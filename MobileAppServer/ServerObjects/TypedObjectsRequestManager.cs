using MobileAppServer.CoreServices;
using MobileAppServer.CoreServices.ControllerManagement;
using MobileAppServer.CoreServices.DomainModelsManagement;
using MobileAppServer.CoreServices.Logging;
using MobileAppServer.ManagedServices;
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
        private ILoggingService logger = null;
        private IControllerManager controllerManager = null;
        private IDomainModelsManager modelsManager = null;
        public TypedObjectsRequestManager()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            logger = manager.GetService<ILoggingService>();
            controllerManager = manager.GetService<IControllerManager>();
            modelsManager = manager.GetService<IDomainModelsManager>();
        }

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
            try
            {
                ControllerRegister register = controllerManager.GetControllerRegister(controllerName);

                Type type = register.Type;
                MethodInfo method = type.GetMethods().FirstOrDefault(m => m.Name.Equals(actionName));
                if (method == null)
                    return null;

                if (!requestParameter.Contains("."))
                    return null;

                string parameter = requestParameter.Substring(0, requestParameter.IndexOf("."));

                ParameterInfo parameterInfo = method.GetParameters().FirstOrDefault(p => p.Name.Equals(parameter));
                ObjectRequestParameter objectParameter = new ObjectRequestParameter(parameterInfo.Name, parameterInfo.ParameterType.FullName);
                return objectParameter;
            }
            catch (Exception ex)
            {
                logger.WriteLog($"*** CHECK PARAMETER '{requestParameter}' FOR ACTION '{controllerName}.{actionName}' ERROR: *** \n{ex.Message}",
                    controllerName, actionName, ServerLogType.ERROR);
                return null;
            }
        }

        internal bool ExistsAction(string actionName, string controllerName)
        {
            logger.WriteLog($"Checking action '{controllerName}.{actionName}'...",
                controllerName, actionName);

            try
            {
                ControllerRegister register = controllerManager.GetControllerRegister(controllerName);

                Type type = register.Type;
                MethodInfo method = type.GetMethod(actionName);

                return true;
            }
            catch (Exception ex)
            {
                logger.WriteLog($"*** CHECK ACTION '{controllerName}.{actionName}' ERROR: *** \n {ex.Message}",
                    controllerName, actionName, ServerLogType.ERROR);
                return false;
            }
        }

        internal List<RequestParameter> GetParameters(List<RequestParameter> parameters, string action,
          IController controller)
        {
            logger.WriteLog($"Checking parameter list form action '{controller.GetType().Name}.{action}'...",
                controller.GetType().Name, action);

            if (parameters == null)
                return new List<RequestParameter>();

            string currentObjRpAlias = string.Empty;
            Object entityParameterObject = this;
            List<RequestParameter> result = new List<RequestParameter>();
            MethodInfo method = controller.GetType().GetMethod(action);
            foreach (RequestParameter parameter in parameters)
            {
                logger.WriteLog($"Setting parameter '{parameter.Name}' for action '{controller.GetType().Name}.{action}'...", 
                    controller.GetType().Name, action);

                string pName = parameter.Name;
                string pValue = parameter.Value.ToString();

                RequestParameter requestParameter = null;

                foreach (ParameterInfo pInfo in method.GetParameters())
                {
                    ObjectRequestParameter objectRequestParameter = GetObjectParameterType(pName, action, controller.GetType().Name);
                    if (objectRequestParameter == null)
                        if (!pInfo.Name.Equals(pName))
                            continue;

                    if (objectRequestParameter != null)
                    {
                        if (objectRequestParameter.Alias != currentObjRpAlias ||
                            entityParameterObject.GetType().Name.Equals(this.GetType().Name))
                        {
                            currentObjRpAlias = objectRequestParameter.Alias;
                            ModelRegister model = modelsManager.GetModelRegister(objectRequestParameter.TypeName);

                            if (model == null)
                            {
                                logger.WriteLog($"Model type '{objectRequestParameter.TypeName} not found or not registered'", controller.GetType().Name, action, 
                                    ServerLogType.ERROR);
                                throw new Exception($"Model type '{objectRequestParameter.TypeName} not found or not registered'");
                            }

                            Type type = model.ModelType;
                            if (type == null)
                            {
                                logger.WriteLog($"Model type '{objectRequestParameter.TypeName} not found or not registered'", controller.GetType().Name, 
                                    action, ServerLogType.ERROR);
                                throw new Exception($"Model type '{objectRequestParameter.TypeName} not found or not registered'");
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
