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
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.ManagedServices;
using SocketAppServer.TelemetryServices;
using SocketAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace SocketAppServer.ServerObjects
{
    internal class ServerInfoController : IController
    {
        private IControllerManager controllerManager = null;
        private ICoreServerService coreServer = null;
        private IHardwareServices hardware = null;
        private ITelemetryServicesProvider telemetry = null;
        public ServerInfoController()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            controllerManager = manager.GetService<IControllerManager>();
            coreServer = manager.GetService<ICoreServerService>("realserver");
            hardware = manager.GetService<IHardwareServices>();
            telemetry = manager.GetService<ITelemetryServicesProvider>();
        }

        [ServerAction]
        public List<string> GetActionParameters(string controller,
            string action)
        {
            ControllerRegister register = controllerManager.GetControllerRegister(controller);
            if (register == null)
                return new List<string>();
            MethodInfo method = register.Type.GetMethod(action);
            if (method == null)
                return new List<string>();
            return method
                .GetParameters()
                .Select(p => p.Name)
                .ToList();
        }

        public ActionResult Reboot()
        {
            coreServer.Reboot();
            return ActionResult.Json(true, 600, "Ok");
        }

        public ActionResult GetCurrentThreadsCount()
        {
            return ActionResult.Json(new OperationResult(RequestProcessor.ThreadCount, 600, null));
        }

        public ActionResult AverageCPUUsage()
        {
            return ActionResult.Json(new OperationResult(hardware.AverageCPUUsage(), 600, null));
        }

        public ActionResult AverageMemoryUsage()
        {
            return ActionResult.Json(new OperationResult(hardware.AverageMemoryUsage(), 600, null));
        }

        public ActionResult RequestsSuccessCount(string controllerName, string actionName)
        {
            int count = (telemetry == null
                ? 0
                : telemetry.RequestsSuccessCount(controllerName, actionName));
            return ActionResult.Json(new OperationResult(count, 600, null));
        }

        public ActionResult RequestsErrorsCount(string controllerName, string actionName)
        {
            int count = (telemetry == null
                ? 0
                : telemetry.RequestErrorsCount(controllerName, actionName));
            return ActionResult.Json(new OperationResult(count, 600, null));
        }

        public ActionResult FullServerInfo()
        {
            ServerInfo info = new ServerInfo();

            foreach (ControllerRegister controller in controllerManager.GetRegisteredControllers())
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
                ControllerRegister register = controllerManager.GetControllerRegister(controllerName);

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
                ControllerRegister register = controllerManager.GetControllerRegister(controllerName);

                Type type = register.Type;
                foreach (var method in type.GetMethods())
                {
                    if (method.GetCustomAttribute<NotListed>() != null)
                        continue;

                    if (method.ReturnType == typeof(ActionResult) ||
                        method.GetCustomAttribute<ServerAction>() != null)
                        result.Add(method.Name);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
    }
}
