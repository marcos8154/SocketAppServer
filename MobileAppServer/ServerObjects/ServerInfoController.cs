using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace MobileAppServer.ServerObjects
{
    internal class ServerInfoController : IController
    {
        public ActionResult Reboot()
        {
            Server.GlobalInstance.SendReboot();
            return ActionResult.Json(true, 600, "Ok");
        }

        public ActionResult GetCurrentThreadsCount()
        {
            return ActionResult.Json(RequestProccess.ThreadCount);
        }

        public ActionResult FullServerInfo()
        {
            ServerInfo info = new ServerInfo();

            foreach (var controller in Server.GlobalInstance.RegisteredControllers)
            {
                ControllerInfo controllerInfo = new ControllerInfo();
                controllerInfo.ControllerName = controller.Name;

                info.ServerControllers.Add(GetControllerInfo(controller.Name));
            }

            return ActionResult.Json(new OperationResult(info, 600, "Server info"));
        }

        public ActionResult DownloadFile(string path)
        {
            return ActionResult.File(path);
        }

        private ControllerInfo GetControllerInfo(string controllerName)
        {
            try
            {
                ControllerRegister register = Server
                 .GlobalInstance
                 .RegisteredControllers
                 .FirstOrDefault(c => c.Name.Equals(controllerName));

                Type type = register.Type;

                ControllerInfo info = new ControllerInfo();
                info.ControllerName = controllerName;
                info.ControllerActions = ListActions(controllerName);
                info.ControllerClass = type.FullName;

                return info;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<string> ListActions(string controllerName)
        {
            List<string> result = new List<string>();
            try
            {
                ControllerRegister register = Server
                 .GlobalInstance
                 .RegisteredControllers
                 .FirstOrDefault(c => c.Name.Equals(controllerName));

                Type type = register.Type;
                foreach (var method in type.GetMethods())
                    if (method.ReturnType == typeof(ActionResult))
                        result.Add(method.Name);

                return result;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
    }
}
