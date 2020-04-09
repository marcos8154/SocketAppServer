using MobileAppServer.CoreServices.Logging;
using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;
using MobileAppServer.TelemetryServices;
using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.ControllerManagement
{
    public class ControllerManagerImpl : IControllerManager
    {
        private IServiceManager serviceManager = null;
        private ITelemetryDataCollector telemetry;
        private List<ControllerRegister> controllers = null;

        public ControllerManagerImpl()
        {
            controllers = new List<ControllerRegister>();
            serviceManager = ServiceManager.GetInstance();
            telemetry = serviceManager.GetService<ITelemetryDataCollector>();
        }

        public ControllerRegister GetControllerRegister(string name)
        {
            return controllers.FirstOrDefault(c => c.Name.Equals(name));
        }

        public void RegisterAllControllers(Assembly assembly, string namespaceName)
        {
            Type[] controllers = GetTypesInNamespace(assembly, namespaceName);
            for (int i = 0; i < controllers.Length; i++)
                RegisterController(controllers[i]);
        }

        public void RegisterController(Type type)
        {
            controllers.Add(new ControllerRegister() { Name = type.Name, Type = type });
        }

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        public IController InstantiateController(string name, RequestBody requestBody)
        {
            try
            {
                ControllerRegister register = GetControllerRegister(name);
                IDependencyInjectorMaker injector = null;

                object[] injectArgs = null;
                if (!name.Equals("ServerInfoController"))
                {
                    IDependencyInjectionService diService = serviceManager.GetService<IDependencyInjectionService>();
                    injector = diService.GetInjectorMaker(name);

                    if (injector != null)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        injectArgs = injector.BuildInjectValues(requestBody);
                        sw.Stop();

                        telemetry.Collect(new DependencyInjectorExecutionTime(injector.ControllerName, sw.ElapsedMilliseconds));
                    }
                }

                IController controller = (IController)Activator.CreateInstance(register.Type, injectArgs);
                return controller;
            }
            catch (Exception ex)
            {
                ILoggingService logger = serviceManager.GetService<ILoggingService>();
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += ex.InnerException.Message;
                logger.WriteLog($@"Instantiate controller '{name}' threw an exception. 
{msg}", name, "", ServerLogType.ERROR);
                throw new Exception($@"Instantiate controller '{name}' threw an exception. 
{msg}");
            }
        }

        public IReadOnlyCollection<ControllerRegister> GetRegisteredControllers()
        {
            return controllers.AsReadOnly();
        }
    }
}
