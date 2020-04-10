/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using MobileAppServer.CoreServices.ControllerManagement;
using MobileAppServer.CoreServices.Logging;
using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;
using MobileAppServer.TelemetryServices;
using MobileAppServer.TelemetryServices.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace MobileAppServer.CoreServices.CoreServer
{
    public class RequestProcessor : AsyncTask<int, int, object>
    {
        internal static object lck = new object();
        internal static int ThreadCount = 0;

        private ILoggingService logger = null;
        private IControllerManager controllerManager = null;
        private IInterceptorManagerService interceptorManager;
        private ITelemetryDataCollector telemetry;
        private IController controller = null;
        private Socket socket;
        private MethodInfo method = null;
        private TypedObjectsRequestManager typedObjManager = new TypedObjectsRequestManager();
        private RequestBody RequestBody { get; set; }

        #region constructor
        internal RequestProcessor(string uriRequest, Socket clientSocket)
        {
            IServiceManager serviceManager = ServiceManager.GetInstance();
            logger = serviceManager.GetService<ILoggingService>();
            controllerManager = serviceManager.GetService<IControllerManager>();
            interceptorManager = serviceManager.GetService<IInterceptorManagerService>();
            telemetry = serviceManager.GetService<ITelemetryDataCollector>();

            socket = clientSocket;
            try
            {
                RequestBody = JsonConvert.DeserializeObject<RequestBody>(uriRequest);
                socket = clientSocket;
                if (RequestBody == null)
                    return;

                string log = $@"

******** BEGIN REQUEST ********* 
Controller: {RequestBody.Controller}
Action: {RequestBody.Action}";
                log += @"
********************************";

                logger.WriteLog(log, RequestBody.Controller, RequestBody.Action);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        msg += ex.InnerException.InnerException.Message;
                }
                new SocketRequest().ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR,
                    $@"A syntax error was detected while converting the request body to a server object. 
{msg}"), socket);
            }
        }
        #endregion

        public override object DoInBackGround(int p)
        {
            SocketRequest request = null;
            string controllerName = null;
            string actionName = null;
            try
            {
                request = GetRequestSocket();
                if (request == null)
                    return string.Empty;

                if (HasRequestErros(request))
                    return string.Empty;

                controller = request.Controller;
                method = controller.GetType().GetMethod(request.Action);

                controllerName = controller.GetType().Name;
                actionName = method.Name;

                if (ActionHasLock(request, controller, method.Name))
                    return string.Empty;

                ResolveActionParameters(request);
                if (!ResolveInterceptors(request))
                    return string.Empty;

                object[] methodParameters = new object[request.Parameters.Count];
                for (int i = 0; i < request.Parameters.Count; i++)
                    methodParameters[i] = request.Parameters[i].Value;

                logger.WriteLog($"Invoking action '{controllerName}.{actionName}'...", controllerName, actionName);
                Stopwatch w = new Stopwatch();
                w.Start();
                ActionResult result = (ActionResult)method.Invoke(controller, methodParameters);
                w.Stop();

                telemetry.Collect(new ActionExecutionTime(controllerName, actionName, w.ElapsedMilliseconds));

                if (w.ElapsedMilliseconds > 10000)
                    ServerAlertManager.CreateAlert(new ServerAlert(request.Controller.GetType().Name, request.Action,
                        $"The action it is taking considerable time to execute ({w.ElapsedMilliseconds} ms). Review your code to improve performance.'"));

                ActionLocker.ReleaseLock(controller, method.Name);

                logger.WriteLog($"Request completed in {w.ElapsedMilliseconds}ms", controller.GetType().Name, method.Name);
                request.ProcessResponse(result, socket);

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += $" {ex.InnerException.Message}";
                if (controller != null &&
                    method != null)
                    ActionLocker.ReleaseLock(controller, method.Name);

                telemetry.Collect(new ActionError(controllerName, actionName, msg));

                if (request != null)
                    request.ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR, $"Process request error: {msg}"), socket);
                return string.Empty;
            }
        }

        #region support methods
        private SocketRequest GetRequestSocket()
        {
            if (RequestBody == null)
                return null;
            if (RequestBody != null)
                if (RequestBody.Controller != null)
                    logger.WriteLog($"Processing request for action '{RequestBody.Controller}.{RequestBody.Action}'...", RequestBody.Controller, RequestBody.Action);
            SocketRequest request = null;
            string controller = "";
            string action = "";
            try
            {
                string path = RequestBody.Action;
                controller = RequestBody.Controller;
                action = RequestBody.Action;

                request = new SocketRequest();
                request.ClientSocket = socket;
                request.Action = action;
                request.Controller = controllerManager.InstantiateController(controller, RequestBody);

                var parameters = typedObjManager.GetParameters(RequestBody.Parameters,
                action, request.Controller);
                parameters.ForEach(p => request.AddParameter(p));

                if (!typedObjManager.ExistsAction(action, controller))
                    throw new Exception($"Action '{action}' not exists in controller '{controller}'");

                return request;
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message + "\n" + ex.StackTrace, controller, action, ServerLogType.ERROR);
                request.HasErrors = true;
                request.InternalErrorMessage = ex.Message;
                return request;
            }
        }

        private bool ResolveInterceptors(SocketRequest request)
        {
            var fakeSocketRequest = new SocketRequest(request.Controller, request.Action, RequestBody.Parameters, socket);
            //GLOBAL Interceptors
            var globalInterceptors = interceptorManager.GlobalServerInterceptors();
            if (!interceptorManager.PreHandleInterceptors(globalInterceptors.ToList(), fakeSocketRequest, socket))
                return false;

            //CONTROLLER (all actions) Interceptors
            var controllerInterceptors = interceptorManager.ControllerInterceptors(controller.GetType().Name);
            if (!interceptorManager.PreHandleInterceptors(controllerInterceptors.ToList(), fakeSocketRequest, socket))
                return false;

            //CONTROLLER (specific action) Interceptors
            var controllerActionInterceptors = interceptorManager.ControllerActionInterceptors(controller.GetType().Name, request.Action);
            if (!interceptorManager.PreHandleInterceptors(controllerActionInterceptors.ToList(), fakeSocketRequest, socket))
                return false;

            return true;
        }

        private void ResolveActionParameters(SocketRequest request)
        {
            foreach (ParameterInfo pi in method.GetParameters())
            {
                if (pi.ParameterType == typeof(SocketRequest))//.Equals("MobileAppServer.ServerObjects.SocketRequest"))
                {
                    request.AddParameter(new RequestParameter("request", request));
                    continue;
                }

                if (!request.Parameters.Any(rp => rp.Name.Equals(pi.Name)))
                    throw new Exception($"O parâmetro '{pi.Name}', necessário em {request.Controller.GetType().Name}/{request.Action}, não foi informado.");
            }
        }

        private bool HasRequestErros(SocketRequest request)
        {
            if (request.HasErrors)
            {
                request.ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR, request.InternalErrorMessage), socket);
                return true;
            }
            return false;
        }

        private bool ActionHasLock(SocketRequest request, IController controller, string actionName)
        {
            if (ActionLocker.ActionHasLock(controller, actionName))
            {
                request.ProcessResponse(ActionResult.Json("", ResponseStatus.LOCKED, $"This action is already being performed by another remote client and is currently blocked. Try again later"), socket);
                return true;
            }
            return false;
        }

        public static void UpThreadCount()
        {
            lock (lck)
            {
                ThreadCount += 1;
            }
        }

        public static void DownThreadCount()
        {
            lock (lck)
            {
                ThreadCount -= 1;
            }
        }

        public override void OnPostExecute(object result)
        {
            if (controller != null && method != null)
                ActionLocker.ReleaseLock(controller, method.Name);
            DownThreadCount();
        }

        public override void OnPreExecute()
        {
            UpThreadCount();
        }

        public override void OnProgressUpdate(int progress)
        {
            // throw new NotImplementedException();
        }
        #endregion
    }
}
