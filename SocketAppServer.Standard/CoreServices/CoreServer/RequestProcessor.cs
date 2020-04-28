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

using SocketAppServer.CoreServices.ControllerManagement;
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using SocketAppServer.TelemetryServices;
using SocketAppServer.TelemetryServices.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace SocketAppServer.CoreServices.CoreServer
{
    /// <summary>
    /// Standard request processor. Supports all server resources such as dependency injection, interceptors and more.
    /// In addition to dynamically instantiating any type of Controller and injecting any type of parameter into Actions.
    /// </summary>
    public class RequestProcessor : AsyncTask<int, int, object>
    {
        internal static object lck = new object();
        internal static int ThreadCount = 0;

        private ILoggingService logger = null;
        private IControllerManager controllerManager = null;
        private IInterceptorManagerService interceptorManager;
        private ITelemetryDataCollector telemetry;
        private IController controller = null;
        private MethodInfo method = null;

        private Socket clientSocket;
        private TypedObjectsRequestManager typedObjManager = null;
        private RequestBody requestBody = null;

        private string controllerName = null;
        private string actionName = null;

        #region constructor
        internal RequestProcessor(ref string uriRequest, ref Socket clientSocket)
        {
            IServiceManager serviceManager = ServiceManager.GetInstance();
            logger = serviceManager.GetService<ILoggingService>();
            controllerManager = serviceManager.GetService<IControllerManager>();
            interceptorManager = serviceManager.GetService<IInterceptorManagerService>();
            telemetry = serviceManager.GetService<ITelemetryDataCollector>();

            typedObjManager = new TypedObjectsRequestManager();
            this.clientSocket = clientSocket;
            try
            {
                using (StringReader sr = new StringReader(uriRequest))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer ser = new JsonSerializer();
                        requestBody = ser.Deserialize<RequestBody>(reader);
                    }
                }

                this.clientSocket = clientSocket;
                if (requestBody == null)
                    return;

                uriRequest = null;
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

                logger.WriteLog($@"A syntax error was detected while converting the request body to a server object. 
{msg}", ServerLogType.ERROR);
                new SocketRequest().ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR,
                    $@"A syntax error was detected while converting the request body to a server object. 
{msg}"), this.clientSocket);
            }
        }
        #endregion

        private void ValidateActionMethod()
        {
            if (method == null)
                throw new Exception($"Not found Action '{requestBody.Action}' in Controller '{requestBody.Controller}'. If so, check if the method returns ActionResult or the method is marked with an [ServerAction] attribute");

            if (method.ReturnType != typeof(ActionResult) &&
                method.GetCustomAttribute<ServerAction>() == null)
                throw new Exception($"Not found Action '{method.Name}' in Controller '{controllerName}'. If so, check if the method returns ActionResult or the method is marked with an [ServerAction] attribute");
        }

        public override object DoInBackGround(int p)
        {
            SocketRequest request = null;

            try
            {
                request = GetRequestSocket();
                if (request == null)
                    return null;

                if (HasRequestErros(ref request))
                    return null;

                controller = request.Controller;
                method = controller.GetType().GetMethod(request.Action);

                ValidateActionMethod();

                controllerName = controller.GetType().Name;
                actionName = method.Name;

                WaitActionLockPendingCompletations(ref request,
                     ref controller, ref actionName);

                if (method.GetCustomAttribute<SingleThreaded>() != null)
                    ActionLocker.AddLock(controller, actionName);

                ResolveActionParameters(ref request);
                if (!ResolveInterceptors(ref request))
                    return null;

                ParameterInfo[] methodParameters = method.GetParameters();
                object[] parameterValues = new object[methodParameters.Length];
                int methodParameterCount = methodParameters
                    .Where(pr => request.Parameters.Any(rp => rp.Name.Equals(pr.Name)))
                    .Count();

                int parameterIndex = 0;
                foreach (RequestParameter rp in request.Parameters)
                {
                    ParameterInfo parameterInfo = methodParameters.FirstOrDefault(mp => mp.Name.Equals(rp.Name));
                    if (parameterInfo == null)
                        continue;

                    parameterValues[parameterIndex] = request.Parameters[parameterIndex].Value;
                    parameterIndex += 1;
                }

                ActionResult result = null;
                Stopwatch w = new Stopwatch();
                w.Start();

                if (method.ReturnType == null)
                {
                    //void action
                    method.Invoke(controller, parameterValues);
                    result = ActionResult.Json(new OperationResult(null, 600, ""));
                }
                else if (method.ReturnType == typeof(ActionResult)) //ActionResult action
                    result = (ActionResult)method.Invoke(controller, parameterValues);
                else
                {
                    //user-defined object Action
                    object returnObj = method.Invoke(controller, parameterValues);
                    result = ActionResult.Json(new OperationResult(returnObj, 600, ""));
                }

                w.Stop();

                if (telemetry != null)
                    telemetry.Collect(new ActionExecutionTime(controllerName, actionName, w.ElapsedMilliseconds));

                logger.WriteLog($"Request completed: {controllerName}/{actionName}; ~{w.ElapsedMilliseconds}ms");

                if (w.ElapsedMilliseconds > 10000)
                    ServerAlertManager.CreateAlert(new ServerAlert(request.Controller.GetType().Name, request.Action,
                        $"The action it is taking considerable time to execute ({w.ElapsedMilliseconds} ms). Review your code to improve performance.'"));

                ActionLocker.ReleaseLock(controller, method.Name);
                request.ProcessResponse(result, clientSocket);
                return result;
            }
            catch (LockedActionException lockedEx)
            {
                ProcessErrorResponse(lockedEx, ref request);
                return null;
            }
            catch (Exception ex)
            {
                if (method != null &&
                    controller != null)
                    ActionLocker.ReleaseLock(controller, actionName);

                ProcessErrorResponse(ex, ref request);
                return null;
            }
        }

        private void ProcessErrorResponse(Exception ex, ref SocketRequest request)
        {
            string msg = ex.Message;
            if (ex.InnerException != null)
                msg += $" {ex.InnerException.Message}";

            if (telemetry != null)
                telemetry.Collect(new ActionError(controllerName, actionName, msg));

            if (request != null)
            {
                ServerAction serverAction = method.GetCustomAttribute<ServerAction>();
                if (serverAction == null)
                    request.ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR, $"Process request error: {msg}"), clientSocket);
                else
                {
                    int errorCode = (serverAction.DefaultErrorCode == 0
                        ? ResponseStatus.ERROR
                        : serverAction.DefaultErrorCode);

                    if (serverAction.ExceptionHandler == null)
                        request.ProcessResponse(ActionResult.Json("", errorCode, $"Process request error: {msg}"), clientSocket);
                    else
                    {
                        IActionExceptionHandler handler = (IActionExceptionHandler)
                            Activator.CreateInstance(serverAction.ExceptionHandler);
                        ActionResult result = ActionResult.Json(handler.Handle(ex, request), 600, "Request error, but handled by application");
                        request.ProcessResponse(result, clientSocket);
                    }
                }
            }
        }

        private void WaitActionLockPendingCompletations(ref SocketRequest request, ref IController controller, ref string actionName)
        {
            if (!ActionLocker.ActionHasLock(controller, actionName))
                return;
            SingleThreaded actionLock = method.GetCustomAttribute<SingleThreaded>();
            if (actionLock == null)
                return;

            for (int i = 0; i < actionLock.WaitingAttempts; i++)
            {
                if (ActionLocker.ActionHasLock(controller, actionName))
                {
                    if ((i + 1) >= actionLock.WaitingAttempts)
                    {
                        throw new LockedActionException("The action does not allow simultaneous access and the maximum number of waiting attempts has been exceeded. Try to access the resource again later.");
                    }

                    Thread.Sleep(actionLock.WaitingInterval);
                }
                else
                    return;
            }

        }

        #region support methods
        private SocketRequest GetRequestSocket()
        {
            if (requestBody == null)
                return null;
            SocketRequest request = null;

            try
            {
                string path = requestBody.Action;
                controllerName = requestBody.Controller;
                actionName = requestBody.Action;

                request = new SocketRequest();
                request.ClientSocket = clientSocket;
                request.Action = actionName;
                request.Controller = controllerManager.InstantiateController(controllerName, requestBody);

                var parameters = typedObjManager.FillParameters(requestBody.Parameters,
                      actionName, request.Controller);

                if (parameters != null)
                    for (int i = 0; i < parameters.Count; i++)
                        request.AddParameter(parameters[i]);

                if (!typedObjManager.ExistsAction(actionName, controllerName))
                    throw new Exception($"Action '{actionName}' not exists in controller '{controllerName}'");

                return request;
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message + "\n" + ex.StackTrace, controllerName, actionName, ServerLogType.ERROR);
                request.HasErrors = true;
                request.InternalErrorMessage = ex.Message;
                return request;
            }
        }

        private bool ResolveInterceptors(ref SocketRequest request)
        {
            var fakeSocketRequest = new SocketRequest(request.Controller, request.Action, requestBody.Parameters, clientSocket);
            //GLOBAL Interceptors
            var globalInterceptors = interceptorManager.GlobalServerInterceptors();
            if (!interceptorManager.PreHandleInterceptors(globalInterceptors.ToList(), fakeSocketRequest, clientSocket))
                return false;

            //CONTROLLER (all actions) Interceptors
            var controllerInterceptors = interceptorManager.ControllerInterceptors(controller.GetType().Name);
            if (!interceptorManager.PreHandleInterceptors(controllerInterceptors.ToList(), fakeSocketRequest, clientSocket))
                return false;

            //CONTROLLER (specific action) Interceptors
            var controllerActionInterceptors = interceptorManager.ControllerActionInterceptors(controller.GetType().Name, request.Action);
            if (!interceptorManager.PreHandleInterceptors(controllerActionInterceptors.ToList(), fakeSocketRequest, clientSocket))
                return false;

            return true;
        }

        private void ResolveActionParameters(ref SocketRequest request)
        {
            foreach (ParameterInfo pi in method.GetParameters())
            {
                if (pi.ParameterType == typeof(SocketRequest))
                {
                    request.AddParameter(new RequestParameter("request", request));
                    continue;
                }

                if (!request.Parameters.Any(rp => rp.Name.Equals(pi.Name)))
                    throw new Exception($"The parameter '{pi.Name}', required on {request.Controller.GetType().Name}/{request.Action}, was not received");
            }
        }

        private bool HasRequestErros(ref SocketRequest request)
        {
            if (request.HasErrors)
            {
                request.ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR, request.InternalErrorMessage), clientSocket);
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
            clientSocket = null;
            typedObjManager = null;
            requestBody = null;
            method = null;
            logger = null;
            controllerManager = null;
            interceptorManager = null;
            telemetry = null;
            controller = null;


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
