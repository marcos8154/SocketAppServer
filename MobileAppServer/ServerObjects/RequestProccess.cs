﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace MobileAppServer.ServerObjects
{
    internal class RequestProccess : AsyncTask<int, int, object>
    {
        private Socket socket;
        // private string uri;
        private TypedObjectsRequestManager typedObjManager = new TypedObjectsRequestManager();
        private RequestBody RequestBody { get; set; }

        private SocketRequest GetRequestSocket()
        {
            if (RequestBody == null)
                return null;
            if (RequestBody != null)
                if (RequestBody.Controller != null)
                    LogController.WriteLog($"Processing request for action '{RequestBody.Controller}.{RequestBody.Action}'...");
            SocketRequest request = null;
            try
            {
                string path = RequestBody.Action;
                string controller = RequestBody.Controller;
                string action = RequestBody.Action;

                request = new SocketRequest();
                request.Client = socket;
                request.Action = action;
                request.Controller = GetController(controller);
                if (!typedObjManager.ExistsAction(action, controller))
                    throw new Exception($"Action '{action}' not exists in controller '{controller}'");

                var parameters = typedObjManager.GetParameters(RequestBody.Parameters,
                    action, request.Controller);
                parameters.ForEach(p => request.AddParameter(p));

                return request;
            }
            catch (Exception ex)
            {
                LogController.WriteLog(ex.Message + "\n" + ex.StackTrace);
                request.HasErrors = true;
                request.InternalErrorMessage = ex.Message;
                return request;
            }
        }

        private IController GetController(string controllerName)
        {
            try
            {
                LogController.WriteLog($"Instantiating Controller {controllerName}...");

                ControllerRegister register = Server.RegisteredControllers.FirstOrDefault(c => c.Name.Equals(controllerName));
                IController controller = (IController)Activator.CreateInstance(register.Type);

                LogController.WriteLog($"Instantiate Controller success! Controller name: {controller.GetType().FullName}");
                return controller;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += ex.InnerException.Message;
                LogController.WriteLog($@"Instantiate controller '{controllerName}' threw an exception. 
{msg}");
                throw new Exception($@"Instantiate controller '{controllerName}' threw an exception. 
{msg}");
            }
        }

        internal RequestProccess(string uriRequest, Socket clientSocket)
        {
            this.socket = clientSocket;
            try
            {
                RequestBody = JsonConvert.DeserializeObject<RequestBody>(uriRequest);
                this.socket = clientSocket;
                if (RequestBody == null)
                    return;

                string log = $@"

******** BEGIN REQUEST ********* 
Controller: {RequestBody.Controller}
Action: {RequestBody.Action}
Parameters: ";
                if (RequestBody.Parameters != null)
                {
                    RequestBody.Parameters.ForEach(p =>
                    {
                        log += $"\n'{p.Name}'='{p.Value}'";
                    });
                }
                log += @"
********************************";

                LogController.WriteLog(log);
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
                    $@"Um erro de sintaxe foi detectado ao converter o corpo da requisição para um objeto do servidor. 
{msg}"), socket);
            }
        }

        private bool PreHandleInterceptors(List<IHandlerInterceptor> interceptors,
            SocketRequest request)
        {
            foreach (var interceptor in interceptors)
            {
                var handleResult = interceptor.PreHandle(request);
                if (!handleResult.Success)
                {
                    request.ProcessResponse(ActionResult.Json("",
                        ResponseStatus.ERROR, handleResult.Message), socket);
                    return false;
                }
            }

            return true;
        }

        IController controller = null;
        MethodInfo method = null;
        public override object DoInBackGround(int p)
        {

            SocketRequest request = null;

            try
            {
                request = GetRequestSocket();
                if (request == null)
                    return string.Empty;
                if (request.HasErrors)
                {
                    request.ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR, request.InternalErrorMessage), socket);
                    return string.Empty;
                }

                controller = request.Controller;
                method = controller.GetType().GetMethod(request.Action);

                if (ActionLocker.ActionHasLock(controller, method.Name))
                {
                    request.ProcessResponse(ActionResult.Json("", ResponseStatus.LOCKED, $"This action is already being performed by another remote client and is currently blocked. Try again later"), socket);
                    return string.Empty;
                }

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

                //GLOBAL Interceptors
                var globalInterceptors = Server.GlobalInstance.Interceptors.Where(i =>
                    i.ActionName.Equals("") && i.ControllerName.Equals("")).ToList();
                if (!PreHandleInterceptors(globalInterceptors, request))
                    return string.Empty;

                //CONTROLLER (all actions) Interceptors
                var controllerInterceptors = Server.GlobalInstance.Interceptors.Where(i =>
                    i.ActionName.Equals("") && i.ControllerName.Equals(controller.GetType().Name)).ToList();
                if (!PreHandleInterceptors(controllerInterceptors, request))
                    return string.Empty;

                //CONTROLLER (specific action) Interceptors
                var controllerActionInterceptors = Server.GlobalInstance.Interceptors.Where(i =>
                    i.ActionName.Equals(method.Name) && i.ControllerName.Equals(controller.GetType().Name)).ToList();
                if (!PreHandleInterceptors(controllerActionInterceptors, request))
                    return string.Empty;

                object[] methodParameters = new object[request.Parameters.Count];
                for (int i = 0; i < request.Parameters.Count; i++)
                    methodParameters[i] = request.Parameters[i].Value;

                LogController.WriteLog($"Invoking action '{controller.GetType().Name}.{method.Name}'...");
                Stopwatch w = new Stopwatch();
                w.Start();
                ActionResult result = (ActionResult)method.Invoke(controller, methodParameters);
                w.Stop();

                ActionLocker.ReleaseLock(controller, method.Name);

                LogController.WriteLog($"Request completed in {w.ElapsedMilliseconds}ms");
                request.ProcessResponse(result, socket);

                return result;
            }
            catch (Exception ex)
            {
                if (controller != null &&
                    method != null)
                    ActionLocker.ReleaseLock(controller, method.Name);

                if (request != null)
                    request.ProcessResponse(ActionResult.Json("", ResponseStatus.ERROR, $"Process request error: \n{ ex.Message}"), socket);
                return string.Empty;
            }
        }

        public static object lck = new object();
        public static int ThreadCount = 0;

        private static void UpThreadCount()
        {
            lock (lck)
            {
                ThreadCount += 1;
            }
        }

        private static void DownThreadCount()
        {
            lock (lck)
            {
                ThreadCount -= 1;
            }
        }

        public override void OnPostExecute(object result)
        {
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
    }
}