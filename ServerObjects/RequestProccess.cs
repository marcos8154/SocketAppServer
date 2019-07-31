using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml;

namespace MobileAppServer.ServerObjects
{
    internal class RequestProccess : AsyncTask<int, int, object>
    {
        private Socket socket;
        // private string uri;
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
                if (!ExistsAction(action, controller))
                    throw new Exception($"Action '{action}' not exists in controller '{controller}'");

                request.Parameters = GetParameters(RequestBody.Parameters,
                    action, request.Controller);
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

        private void ConvertValueRequestToTypedValueEntity(Object entity,
            string propertyName, string requestValue)
        {
            PropertyInfo property = entity.GetType().GetProperty(propertyName);

            if (property.PropertyType == typeof(bool))
                property.SetValue(entity, bool.Parse(requestValue), null);
            if (property.PropertyType == typeof(int))
                property.SetValue(entity, int.Parse(requestValue), null);
            if (property.PropertyType == typeof(string))
                property.SetValue(entity, requestValue, null);
            if (property.PropertyType == typeof(decimal))
                property.SetValue(entity, decimal.Parse(requestValue), null);
            if (property.PropertyType == typeof(double))
                property.SetValue(entity, double.Parse(requestValue), null);
            if (property.PropertyType == typeof(float))
                property.SetValue(entity, float.Parse(requestValue), null);
            if (property.PropertyType == typeof(bool))
                property.SetValue(entity, bool.Parse(requestValue), null);
            if (property.PropertyType == typeof(DateTime))
                property.SetValue(entity, DateTime.Parse(requestValue), null);
            if (property.PropertyType == typeof(double))
                property.SetValue(entity, double.Parse(requestValue), null);
        }

        private List<RequestParameter> GetParameters(List<RequestParameter> parameters, string action,
            IController controller)
        {
            LogController.WriteLog($"Checking parameter list form action '{controller.GetType().Name}.{action}'...");
            if (parameters == null)
                return new List<RequestParameter>();

            string currentObjRpAlias = string.Empty;
            Object entityParameterObject = this;
            List<RequestParameter> result = new List<RequestParameter>();
            MethodInfo method = controller.GetType().GetMethod(action);
            foreach (RequestParameter parameter in parameters)
            {
                LogController.WriteLog($"Setting parameter '{parameter.Name}' for action '{controller.GetType().Name}.{action}'...");
                string pName = parameter.Name;
                string pValue = parameter.Value.ToString();

                RequestParameter requestParameter = null;

                foreach (ParameterInfo pInfo in method.GetParameters())
                {
                    ObjectRequestParameter objRP = GetObjectParameterType(pName, action, controller.GetType().Name);
                    if (objRP == null)
                        if (!pInfo.Name.Equals(pName))
                            continue;

                    // string objectParameterType = objRP.TypeName;

                    if (objRP != null)
                    {
                        LogController.WriteLog("Object parameter is not null!");
                        if (objRP.Alias != currentObjRpAlias ||
                            entityParameterObject.GetType().Name.Equals(this.GetType().Name))
                        {
                            currentObjRpAlias = objRP.Alias;

                            ModelRegister model = Server.RegisteredModels.FirstOrDefault(m => m.ModeName == objRP.TypeName);
                            if (model == null)
                                throw new Exception($"Could not instantiate controller '{objRP.TypeName}'. Check its mapping XML.");

                            Type type = model.ModelType;
                            if (type == null)
                                throw new Exception($"Could not instantiate controller '{objRP.TypeName}'. Check its mapping XML.");
                            entityParameterObject = Activator.CreateInstance(type);

                            string parameterMethodName = pName.Substring(0, pName.IndexOf('.'));
                            requestParameter = new RequestParameter(parameterMethodName, entityParameterObject);
                        }

                        int ptIndex = pName.IndexOf('.') + 1;
                        string propertyName = pName.Substring(ptIndex, pName.Length - ptIndex);

                        ConvertValueRequestToTypedValueEntity(entityParameterObject,
                            propertyName, pValue);

                        continue;
                    }

                    if (pInfo.ParameterType == typeof(bool))
                    {
                        requestParameter = (new RequestParameter(pName, bool.Parse(pValue)));
                        break;
                    }

                    if (pInfo.ParameterType == typeof(int))
                    {
                        requestParameter = (new RequestParameter(pName, int.Parse(pValue)));
                        break;
                    }

                    if (pInfo.ParameterType == typeof(string))
                    {
                        requestParameter = (new RequestParameter(pName, pValue));
                        break;
                    }

                    if (pInfo.ParameterType == typeof(decimal))
                    {
                        requestParameter = (new RequestParameter(pName, decimal.Parse(pValue)));
                        break;
                    }

                    if (pInfo.ParameterType == typeof(double))
                    {
                        requestParameter = (new RequestParameter(pName, double.Parse(pValue)));
                        break;
                    }

                    if (pInfo.ParameterType == typeof(DateTime))
                    {
                        requestParameter = (new RequestParameter(pName, Convert.ToDateTime(pValue)));
                        break;
                    }

                    if (pInfo.ParameterType == typeof(long))
                    {
                        requestParameter = (new RequestParameter(pName, long.Parse(pValue)));
                        break;
                    }
                }

                if (requestParameter != null)
                    result.Add(requestParameter);
            }
            return result;
        }

        private ObjectRequestParameter GetObjectParameterType(string requestParameter, string actionName,
            string controllerName)
        {
            StreamReader stream = null;

            try
            {
                stream = new StreamReader(
                    Directory.GetCurrentDirectory() + $@"\Mappings\{controllerName}.xml", Encoding.UTF8);
                XmlDocument xml = new XmlDocument();
                xml.Load(stream);
                stream.Close();

                XmlNode node = FindNode(xml.ChildNodes, "ControllerMapping");
                foreach (XmlNode requestMappingNode in node.ChildNodes)
                    if (requestMappingNode.Name.Equals("RequestMapping"))
                        foreach (XmlNode requestMappingParameter in requestMappingNode.ChildNodes)
                            if (requestParameter.StartsWith(requestMappingParameter.Attributes["alias"].Value))
                                if (requestMappingParameter.ParentNode.Attributes["value"].Value == actionName)
                                    return new ObjectRequestParameter(requestMappingParameter.Attributes["alias"].Value,
                                        requestMappingParameter.Attributes["entity"].Value);

                return null;
            }
            catch (Exception ex)
            {
                LogController.WriteLog($"*** CHECK PARAMETER '{requestParameter}' FOR ACTION '{controllerName}.{actionName}' ERROR: *** \n{ex.Message}");
                return null;
            }
        }

        public static XmlNode FindNode(XmlNodeList list, string nodeName)
        {
            if (list.Count > 0)
            {
                foreach (XmlNode node in list)
                {
                    if (node.Name.Equals(nodeName)) return node;
                    if (node.HasChildNodes)
                        if (node.FirstChild.Name.Equals(nodeName)) return node.FirstChild;
                    if (node.HasChildNodes) return FindNode(node.ChildNodes, nodeName);
                }
            }
            return null;
        }

        private bool ExistsAction(string actionName, string controllerName)
        {
            LogController.WriteLog($"Checking action '{controllerName}.{actionName}'...");
            StreamReader stream = null;

            try
            {
                stream = new StreamReader(
                    Directory.GetCurrentDirectory() + $@"\Mappings\{controllerName}.xml", Encoding.UTF8);
                XmlDocument xml = new XmlDocument();
                xml.Load(stream);
                stream.Close();
                stream.Dispose();

                XmlNode node = FindNode(xml.ChildNodes, "ControllerMapping");
                foreach (XmlNode chNode in node.ChildNodes)
                    if (chNode.Name.Equals("RequestMapping"))
                        foreach (XmlAttribute a in chNode.Attributes)
                            if (a.Name.Equals("value") && a.Value.Equals(actionName))
                                return true;

                return false;
            }
            catch (Exception ex)
            {
                LogController.WriteLog($"*** CHECK ACTION '{controllerName}.{actionName}' ERROR: *** \n {ex.Message}");
                return false;
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

        public override object DoInBackGround(int p)
        {
            IController controller = null;
            MethodInfo method = null;
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
                        request.Parameters.Add(new RequestParameter("request", request));
                        continue;
                    }

                    if (!request.Parameters.Any(rp => rp.Name.Equals(pi.Name)))
                        throw new Exception($"O parâmetro '{pi.Name}', necessário em {request.Controller.GetType().Name}/{request.Action}, não foi informado.");
                }

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
            lock(lck)
            {
                ThreadCount += 1;
            }
        }

        private static void DownThreadCount()
        {
            lock(lck)
            {
                ThreadCount -= 1;
            }
        }

        public override void OnPostExecute(object result)
        {
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
