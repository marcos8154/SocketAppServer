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

using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.ControllerManagement;
using SocketAppServer.CoreServices.DomainModelsManagement;
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.ManagedServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace SocketAppServer.ServerObjects
{
    internal class TypedObjectsRequestManager
    {
        private ILoggingService logger = null;
        private IControllerManager controllerManager = null;
        private IDomainModelsManager modelsManager = null;
        private IEncodingConverterService encoder = null;
        public TypedObjectsRequestManager()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            logger = manager.GetService<ILoggingService>();
            controllerManager = manager.GetService<IControllerManager>();
            modelsManager = manager.GetService<IDomainModelsManager>();
            encoder = manager.GetService<IEncodingConverterService>();
        }

        internal static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
              || type.Equals(typeof(string)) ||
              type.Equals(typeof(System.Decimal)) ||
              type.Equals(typeof(System.Int32)) ||
              type.Equals(typeof(System.String)) ||
              type.Equals(typeof(System.Int16)) ||
              type.Equals(typeof(System.Double)) ||
              type.Equals(typeof(System.Guid)) ||
              type.Equals(typeof(System.DateTime)) ||
              type.Equals(typeof(System.Byte));
        }

        internal void FillProperty(object entity,
              string propertyName, string requestValue)
        {
            PropertyInfo property = entity.GetType().GetProperty(propertyName);

            if (property.PropertyType == typeof(bool))
                property.SetValue(entity, bool.Parse(requestValue), null);

            if (property.PropertyType == typeof(int))
                property.SetValue(entity, int.Parse(requestValue), null);

            if (property.PropertyType == typeof(string))
                property.SetValue(entity, requestValue, null);

            if (property.PropertyType == typeof(char))
                property.SetValue(entity, char.Parse(requestValue), null);

            if (property.PropertyType == typeof(decimal))
                property.SetValue(entity, decimal.Parse(requestValue), null);

            if (property.PropertyType == typeof(long))
                property.SetValue(entity, long.Parse(requestValue), null);

            if (property.PropertyType == typeof(float))
                property.SetValue(entity, float.Parse(requestValue), null);

            if (property.PropertyType == typeof(byte[]))
                property.SetValue(entity, encoder.ConvertToByteArray(requestValue), null);

            if (property.PropertyType == typeof(short))
                property.SetValue(entity, short.Parse(requestValue), null);

            if (property.PropertyType == typeof(DateTime))
                property.SetValue(entity, DateTime.Parse(requestValue), null);

            if (property.PropertyType == typeof(double))
                property.SetValue(entity, double.Parse(requestValue), null);

            if (property.PropertyType == typeof(Guid))
                property.SetValue(entity, Guid.Parse(requestValue), null);

            if (property.PropertyType == typeof(List<>) ||
             property.PropertyType.Name.Contains("List"))
            {
                var type = property.PropertyType.GenericTypeArguments[0];
                Type typeList = typeof(List<>).MakeGenericType(type);
                object list = JsonConvert.DeserializeObject(requestValue, typeList);
                property.SetValue(entity, list, null);
            }
        }

        internal RequestParameter GetRequestParameterFromType(ParameterInfo pInfo,
         string pName, string pValue)
        {
            bool boolResult;
            int intResult;
            long longResult;
            char charResult;
            decimal decimalResult;
            double doubleResult;
            short shortResult;
            DateTime dateTimeResult;
            Guid guidResult;

            if (pInfo.ParameterType == typeof(string))
                return (new RequestParameter(pName, pValue));

            if (pInfo.ParameterType == typeof(bool))
                if (bool.TryParse(pValue, out boolResult))
                    return (new RequestParameter(pName, boolResult));

            if (pInfo.ParameterType == typeof(int))
                if (int.TryParse(pValue, out intResult))
                    return (new RequestParameter(pName, intResult));

            if (pInfo.ParameterType == typeof(long))
                if (long.TryParse(pValue, out longResult))
                    return (new RequestParameter(pName, longResult));

            if (pInfo.ParameterType == typeof(char))
                if (char.TryParse(pValue, out charResult))
                    return (new RequestParameter(pName, charResult));

            if (pInfo.ParameterType == typeof(decimal))
                if (decimal.TryParse(pValue, out decimalResult))
                    return (new RequestParameter(pName, decimalResult));

            if (pInfo.ParameterType == typeof(double))
                if (double.TryParse(pValue, out doubleResult))
                    return (new RequestParameter(pName, doubleResult));

            if (pInfo.ParameterType == typeof(short))
                if (short.TryParse(pValue, out shortResult))
                    return (new RequestParameter(pName, shortResult));

            if (pInfo.ParameterType == typeof(byte[]))
                return (new RequestParameter(pName, encoder.ConvertToByteArray(pValue)));

            if (pInfo.ParameterType == typeof(DateTime))
                if (DateTime.TryParse(pValue, out dateTimeResult))
                    return new RequestParameter(pName, dateTimeResult);

            if (pInfo.ParameterType == typeof(Guid))
                if (Guid.TryParse(pValue, out guidResult))
                    return new RequestParameter(pName, guidResult);

            if (pInfo.ParameterType == typeof(List<>) ||
               pInfo.ParameterType.Name.Contains("List"))
            {
                var type = pInfo.ParameterType.GenericTypeArguments[0];
                Type typeList = typeof(List<>).MakeGenericType(type);
                object list = null;

                using (StringReader sr = new StringReader(pValue))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer ser = new JsonSerializer();
                        list = ser.Deserialize(reader, typeList);
                    }
                }

                return new RequestParameter(pName, list);
            }

            return null;
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
                return new ObjectRequestParameter(parameterInfo.Name, parameterInfo.ParameterType.FullName);
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

        internal List<RequestParameter> FillParameters(List<RequestParameter> parameters, string action,
           IController controller)
        {
            if (parameters == null)
                return null;

            if (parameters.Select(p => p.Name).Any(n => n.Contains(".")))
                return LegacyFillParameters(ref parameters, ref action, ref controller);
            else
                return NewFillParameters(ref parameters, ref action, ref controller);
        }

        private List<RequestParameter> NewFillParameters(ref List<RequestParameter> parameters,
            ref string action, ref IController controller)
        {
            MethodInfo method = controller.GetType().GetMethod(action);
            foreach (RequestParameter requestParameter in parameters)
            {
                try
                {
                    ParameterInfo parameterInfo = method
                        .GetParameters()
                        .FirstOrDefault(p => p.Name.Equals(requestParameter.Name));

                    if (parameterInfo == null)
                        continue;

                    if (IsSimpleType(parameterInfo.ParameterType) ||
                        parameterInfo.ParameterType == typeof(List<>) ||
                        parameterInfo.ParameterType.Name.Contains("List"))
                    {
                        RequestParameter tmpParameter = GetRequestParameterFromType(parameterInfo,
                            requestParameter.Name, requestParameter.Value + "");
                        requestParameter.Value = tmpParameter.Value;
                    }
                    else
                    {
                        ModelRegister model = modelsManager.GetModelRegister(parameterInfo.ParameterType.ToString());
                        if (model == null)
                            throw new Exception($"Model type '{parameterInfo.ParameterType}', in '{controller.GetType().Name}/{action}' not found or not registered");

                        using (StringReader sr = new StringReader(requestParameter.Value + ""))
                        {
                            using (JsonReader jr = new JsonTextReader(sr))
                            {
                                JsonSerializer js = new JsonSerializer();
                                requestParameter.Value = js.Deserialize(jr, parameterInfo.ParameterType);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.WriteLog($"Could not convert parameter '{requestParameter.Name}', required in action '{action}' of controller '{controller.GetType().Name}'. The reason is: {ex.Message}", ServerLogType.ERROR);
                    requestParameter.Value = null;
                }
            }

            return parameters;
        }

        [Obsolete]
        private List<RequestParameter> LegacyFillParameters(ref List<RequestParameter> parameters,
            ref string action, ref IController controller)
        {
            string msg = "You are using an old version of the native client for the server, or the format of the parameters is out of date. The framework will use the hourly backward compatibility mode, but in future versions support for this parameter format will be discontinued.";
            logger.WriteLog(msg, ServerLogType.ALERT);
            ServerAlertManager.CreateAlert(new ServerAlert(controller.GetType().Name, action, msg));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;

            object entityParameterObject = null;
            HashSet<RequestParameter> result = new HashSet<RequestParameter>();
            MethodInfo method = controller.GetType().GetMethod(action);
            RequestParameter requestParameter = null;
            string currentAlias = null;

            foreach (RequestParameter parameter in parameters)
            {
                string pName = parameter.Name;
                string pValue = Convert.ToString(parameter.Value);

                foreach (ParameterInfo pInfo in method.GetParameters())
                {
                    if (parameter.IsComplexType() && !IsSimpleType(pInfo.ParameterType))
                    {
                        ObjectRequestParameter objectRequestParameter = GetObjectParameterType(pName, action, controller.GetType().Name);
                        if (currentAlias != objectRequestParameter.Alias)
                        {
                            currentAlias = objectRequestParameter.Alias;
                            requestParameter = null;
                            ModelRegister model = modelsManager.GetModelRegister(objectRequestParameter.TypeName);
                            if (model == null)
                                throw new Exception($"Model type '{objectRequestParameter.TypeName}' not found or not registered");

                            //instantiate object parameter (public ctor)
                            entityParameterObject = Activator.CreateInstance(model.ModelType);
                        }
                        //requestParameter for a complex type object
                        if (requestParameter == null)
                            requestParameter = new RequestParameter(parameter.GetAliasName(), entityParameterObject);
                        FillProperty(entityParameterObject, parameter.GetParameterProperyName(), pValue);
                        break;
                    }

                    //requestParameter for simple type object
                    requestParameter = GetRequestParameterFromType(pInfo, pName, pValue);
                    if (requestParameter != null)
                        break;
                }

                if (requestParameter != null)
                    result.Add(requestParameter);
            }
            return result.ToList();
        }
    }
}
