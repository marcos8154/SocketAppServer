using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace MobileAppServer.ServerObjects
{
    internal class TypedObjectsRequestManager
    {
        internal RequestParameter GetRequestParameterFromType(ParameterInfo pInfo,
            string pName, string pValue)
        {
            if (pInfo.ParameterType == typeof(bool))
                return (new RequestParameter(pName, bool.Parse(pValue)));

            if (pInfo.ParameterType == typeof(int))
                return (new RequestParameter(pName, int.Parse(pValue)));

            if (pInfo.ParameterType == typeof(string))
                return (new RequestParameter(pName, pValue));

            if (pInfo.ParameterType == typeof(decimal))
                return (new RequestParameter(pName, decimal.Parse(pValue)));

            if (pInfo.ParameterType == typeof(double))
                return (new RequestParameter(pName, double.Parse(pValue)));

            if (pInfo.ParameterType == typeof(DateTime))
                return new RequestParameter(pName, Convert.ToDateTime(pValue));

            if (pInfo.ParameterType == typeof(long))
                return (new RequestParameter(pName, long.Parse(pValue)));

            if (pInfo.ParameterType == typeof(Guid))
                return new RequestParameter(pName, Guid.Parse(pValue));

            if (pInfo.ParameterType == typeof(List<>) ||
               pInfo.ParameterType.Name.Contains("List"))
            {
                var type = pInfo.ParameterType.GenericTypeArguments[0];
                Type typeList = typeof(List<>).MakeGenericType(type);
                object list = JsonConvert.DeserializeObject(pValue, typeList);
                return new RequestParameter(pName, list);
            }

            return null;
        }

        internal void ConvertToType(object entity,
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
            if (property.PropertyType == typeof(List<>) ||
                property.PropertyType.Name.Contains("List"))
            {
                var type = property.PropertyType.GenericTypeArguments[0];
                Type typeList = typeof(List<>).MakeGenericType(type);
                object list = JsonConvert.DeserializeObject(requestValue, typeList);
                property.SetValue(entity, list, null);
            }
            if (property.PropertyType == typeof(DateTime))
                property.SetValue(entity, DateTime.Parse(requestValue), null);
            if (property.PropertyType == typeof(double))
                property.SetValue(entity, double.Parse(requestValue), null);
            if (property.PropertyType == typeof(Guid))
                property.SetValue(entity, Guid.Parse(requestValue), null);
        }

        internal ObjectRequestParameter GetObjectParameterType(string requestParameter, string actionName,
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
                stream.Dispose();

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
                LogController.WriteLog(new ServerLog($"*** CHECK PARAMETER '{requestParameter}' FOR ACTION '{controllerName}.{actionName}' ERROR: *** \n{ex.Message}",
                    controllerName, actionName, ServerLogType.ERROR));
                return null;
            }
        }

        internal bool ExistsAction(string actionName, string controllerName)
        {
            LogController.WriteLog(new ServerLog($"Checking action '{controllerName}.{actionName}'...",
                controllerName, actionName));

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
                LogController.WriteLog(new ServerLog($"*** CHECK ACTION '{controllerName}.{actionName}' ERROR: *** \n {ex.Message}",
                    controllerName, actionName, ServerLogType.ERROR));
                return false;
            }
        }

        internal XmlNode FindNode(XmlNodeList list, string nodeName)
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

        internal List<RequestParameter> GetParameters(List<RequestParameter> parameters, string action,
          IController controller)
        {
            LogController.WriteLog(new ServerLog($"Checking parameter list form action '{controller.GetType().Name}.{action}'...", controller.GetType().Name, action));
            if (parameters == null)
                return new List<RequestParameter>();

            string currentObjRpAlias = string.Empty;
            Object entityParameterObject = this;
            List<RequestParameter> result = new List<RequestParameter>();
            MethodInfo method = controller.GetType().GetMethod(action);
            foreach (RequestParameter parameter in parameters)
            {
                LogController.WriteLog(new ServerLog($"Setting parameter '{parameter.Name}' for action '{controller.GetType().Name}.{action}'...", controller.GetType().Name, action));
                string pName = parameter.Name;
                string pValue = parameter.Value.ToString();

                RequestParameter requestParameter = null;

                foreach (ParameterInfo pInfo in method.GetParameters())
                {
                    ObjectRequestParameter objectRequestParameter = GetObjectParameterType(pName, action, controller.GetType().Name);
                    if (objectRequestParameter == null)
                        if (!pInfo.Name.Equals(pName))
                            continue;

                    // string objectParameterType = objRP.TypeName;

                    if (objectRequestParameter != null)
                    {
                        if (objectRequestParameter.Alias != currentObjRpAlias ||
                            entityParameterObject.GetType().Name.Equals(this.GetType().Name))
                        {
                            currentObjRpAlias = objectRequestParameter.Alias;

                            ModelRegister model = Server.GlobalInstance.RegisteredModels.FirstOrDefault(m => m.ModeName == objectRequestParameter.TypeName);
                            if (model == null)
                            {
                                LogController.WriteLog(new ServerLog($"Could not instantiate controller '{objectRequestParameter.TypeName}'. Check its mapping XML.", controller.GetType().Name, action, ServerLogType.ERROR));
                                throw new Exception($"Could not instantiate controller '{objectRequestParameter.TypeName}'. Check its mapping XML.");
                            }

                            Type type = model.ModelType;
                            if (type == null)
                            {
                                LogController.WriteLog(new ServerLog($"Could not instantiate controller '{objectRequestParameter.TypeName}'. Check its mapping XML.", controller.GetType().Name, action, ServerLogType.ERROR));
                                throw new Exception($"Could not instantiate controller '{objectRequestParameter.TypeName}'. Check its mapping XML.");
                            }

                            entityParameterObject = Activator.CreateInstance(type);

                            string parameterMethodName = pName.Substring(0, pName.IndexOf('.'));
                            requestParameter = new RequestParameter(parameterMethodName, entityParameterObject);
                        }

                        int ptIndex = pName.IndexOf('.') + 1;
                        string propertyName = pName.Substring(ptIndex, pName.Length - ptIndex);

                        ConvertToType(entityParameterObject,
                            propertyName, pValue);

                        continue;
                    }

                    requestParameter = GetRequestParameterFromType(pInfo, pName, pValue);
                    if (requestParameter != null)
                        break;
                }

                if (requestParameter != null)
                    result.Add(requestParameter);
            }
            return result;
        }

    }
}
