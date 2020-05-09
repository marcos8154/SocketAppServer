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

using SocketAppServer.CoreServices.CLIHost;
using SocketAppServer.CoreServices.ControllerManagement;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.CoreServices.DIManagement;
using SocketAppServer.CoreServices.DomainModelsManagement;
using SocketAppServer.CoreServices.EFIManagement;
using SocketAppServer.CoreServices.InterceptorManagement;
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.CoreServices.ProxyServices;
using SocketAppServer.CoreServices.ScheduledTaskManagement;
using SocketAppServer.CoreServices.SecurityManagement;
using SocketAppServer.CoreServices.TelemetryManagement;
using SocketAppServer.EFI;
using SocketAppServer.LoadBalancingServices;
using SocketAppServer.ManagedServices;
using SocketAppServer.ScheduledServices;
using SocketAppServer.Security;
using SocketAppServer.ServerObjects;
using System;
using System.Linq;
using System.Reflection;

namespace SocketAppServer.CoreServices
{
    public abstract class AppServerConfigurator
    {
        internal IServiceManager Services { get; set; }
        internal static Type DefaultExceptionHandlerType { get; private set; }
        internal static bool DisableStatisticsCalculating { get; private set; }
        public AppServerConfigurator()
        {
            Services = ServiceManager.GetInstance();

            Services.Bind<ILoggingService>(typeof(LoggingServiceImpl), true);
            Services.Bind<IControllerManager>(typeof(ControllerManagerImpl), true);
            Services.Bind<IDomainModelsManager>(typeof(DomainModelsManager), true);
            Services.Bind<IEFIManager>(typeof(EFIManagerImpl), true);
            Services.Bind<ICoreServerService>("realserver", typeof(CoreServerImpl), true);
            Services.Bind<ICoreServerService>(typeof(ProxyCoreServer), true);
            Services.Bind<IHardwareServices>(typeof(HWServiceImpl), true);
            Services.Bind<IInterceptorManagerService>(typeof(InterceptorManagementServiceImpl), true);
            Services.Bind<IDependencyInjectionService>(typeof(DependencyInjectorManagerImpl), true);
            Services.Bind<ISecurityManagementService>(typeof(SecurityManagementServiceImpl), true);
            Services.Bind<IScheduledTaskManager>(typeof(ScheduledTaskManagerImpl), true);
            Services.Bind<IEncodingConverterService>(typeof(ServerEncodingConverterServiceImpl), false);
            Services.Bind<ITelemetryManagement>(typeof(TelemetryManagementImpl), true);
            Services.Bind<ICLIHostService>(typeof(CLIHostServiceImpl), true);
        }

        public abstract void ConfigureServices(IServiceManager serviceManager);
        public abstract ServerConfiguration GetServerConfiguration();

        /// <summary>
        /// Disables the calculation of server buffer usage statistics. 
        /// Disabling this feature will bring performance gains to the 
        /// server, but in return finding the cause of problems in 
        /// Controllers/Actions will become more difficult
        /// </summary>
        protected void DisableStatisticsComputing()
        {
            DisableStatisticsCalculating = true;
        }

        /// <summary>
        /// Configures a standard exception handler for all server actions
        /// that use the annotation/attribute [ServerAction]
        /// </summary>
        /// <param name="exceptionHandlerType">IActionExceptionHandler implementation class type</param>
        protected void ConfigureDefaultExceptionHandler(Type exceptionHandlerType)
        {
            if (exceptionHandlerType == null)
                throw new Exception("Type cannot be null");
            if (!typeof(IActionExceptionHandler).IsAssignableFrom(exceptionHandlerType))
                throw new Exception($"The type '{exceptionHandlerType.FullName}' not implements IActionExceptionHandler");
            DefaultExceptionHandlerType = exceptionHandlerType;
        }

        /// <summary>
        /// Enable the loading of a framework extension (EFI) as soon as the server core is booted
        /// </summary>
        /// <param name="extension"></param>
        protected void EnableExtension(IExtensibleFrameworkInterface extension)
        {
            Services.GetService<IEFIManager>().AddExtension(extension);
        }

        /// <summary>
        /// It allows integration with the CLI of the framework, which allows execution of specific routines from commands typed at the ConsoleApp prompt
        /// </summary>
        /// <param name="commandText">Initial command that triggers the execution client</param>
        /// <param name="commandDescription">Brief description of the purpose of the command</param>
        /// <param name="executorClient">Instance of the implementation of the ICLIClient interface, which connects the CLI of ConsoleApp with the target class that will perform the tasks</param>
        protected void RegisterCLICommand(string commandText, string commandDescription, ICLIClient executorClient)
        {
            Services.GetService<ICLIHostService>().RegisterCLICommand(commandText, commandDescription, executorClient);
        }

        /// <summary>
        /// Enables load balancing features on the current server
        /// </summary>
        /// <param name="maxAttemptsToGetAvailableSubServer">In a pool of sub-servers, determines the maximum number of attempts to obtain an eligible server to meet the request from the connected client</param>
        /// <param name="cacheResultsForUnreachableServers">If the number of attempts to obtain a server from the pool has exceeded, it will return a cache of the last request successfully made in the controller/action</param>
        /// <returns></returns>
        protected LoadBalanceConfigurator EnableLoadBalanceServer(int maxAttemptsToGetAvailableSubServer = 3,
          bool cacheResultsForUnreachableServers = false)
        {
            ICoreServerService coreServer = Services.GetService<ICoreServerService>("realserver");
            coreServer.EnableBasicServerProcessorMode(typeof(LoadBalanceServer));

            if (cacheResultsForUnreachableServers)
                LoadBalanceServer.EnableCachedResultsForUnreachableServers();

            LoadBalanceServer.SetAttemptsToGetSubServerAvailable(maxAttemptsToGetAvailableSubServer);
            return new LoadBalanceConfigurator();
        }

        /// <summary>
        /// Disables standard telemetry services on the server
        /// WARNING!: Disabling telemetry services can bring some extra performance to the server (even if perhaps imperceptible). However it will not be possible to collect metrics to implement improvements in your code
        /// </summary>
        protected void DisableTelemetryServices()
        {
            ICoreServerService coreServer = Services.GetService<ICoreServerService>();
            coreServer.DisableTelemetryServices();
        }

        /// <summary>
        /// Enables authentication features on the current server
        /// </summary>
        /// <param name="repository">Implementation of a ServerUser repository for authentication</param>
        /// <param name="tokenLifetime">Authentication Token lifetime (in minutes)</param>
        /// <param name="tokenCryptPassword">(Optional) A password used to encrypt the data that will make up the authentication Token. If it is an empty string, the authentication service will take care of generating dynamic passwords for each connected client. For the password generation process, see Wiki section on GitHub project</param>
        protected void UseAuthentication(IServerUserRepository repository,
            int tokenLifetime = 3, string tokenCryptPassword = "")
        {
            ISecurityManagementService service = Services.GetService<ISecurityManagementService>();
            service.EnableSecurity(repository, tokenLifetime, tokenCryptPassword);
        }

        /// <summary>
        /// Provide a wrapper for capturing logs fired by the Logging service
        /// </summary>
        /// <param name="loggerWrapper">Implementation of the wrapper that will capture and provide query logs</param>
        protected void SetDefaultLoggerWrapper(ILoggerWrapper loggerWrapper)
        {
            Services.GetService<ILoggingService>().SetWrapper(loggerWrapper);
        }

        /// <summary>
        /// Adds a request interceptor for a specific server controller/action
        /// </summary>
        /// <param name="interceptor">Implementation of Interceptor</param>
        protected void AddInterceptor(IHandlerInterceptor interceptor)
        {
            IInterceptorManagerService manager = Services.GetService<IInterceptorManagerService>();
            manager.AddInterceptor(interceptor);
        }

        /// <summary>
        /// Adds a dependency injection helper for server controllers
        /// </summary>
        /// <param name="dependencyInjector">Implementation of injector helper</param>
        protected void AddDependencyInjector(IDependencyInjectorMaker dependencyInjector)
        {
            IDependencyInjectionService service = Services.GetService<IDependencyInjectionService>();
            service.AddDependencyInjector(dependencyInjector);
        }

        /// <summary>
        /// Register a controller on the server
        /// </summary>
        /// <param name="controllerType">Concrete type of controller. It is mandatory to implement the "IController" interface</param>
        protected void RegisterController(Type controllerType)
        {
            IControllerManager manager = Services.GetService<IControllerManager>();
            manager.RegisterController(controllerType);
        }

        /// <summary>
        /// Adds a set of Controllers to the server. A scan will be performed on the specified Assembly namespace to search for possible Controllers and register them automatically
        /// </summary>
        /// <param name="assembly">Assembly in which Controller classes are contained</param>
        /// <param name="namespaceName">Assembly namespace in which the controllers are contained</param>
        protected void RegisterAllControllers(Assembly assembly, string namespaceName)
        {
            IControllerManager manager = Services.GetService<IControllerManager>();
            manager.RegisterAllControllers(assembly, namespaceName);
        }

        /// <summary>
        /// Registers a model class so that the Controller can inject them into the Actions they need
        /// </summary>
        /// <param name="modelType">Base type of the class to be registered</param>
        protected void RegisterModel(Type modelType)
        {
            IDomainModelsManager manager = Services.GetService<IDomainModelsManager>();
            manager.RegisterModelType(modelType);
        }

        /// <summary>
        /// Adds a set of model classes to the server. A scan will be performed on the specified Assembly namespace to search for possible classes and register them automatically
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="namespaceName"></param>
        protected void RegisterAllModels(Assembly assembly, string namespaceName)
        {
            IDomainModelsManager manager = Services.GetService<IDomainModelsManager>();
            manager.RegisterAllModels(assembly, namespaceName);
        }

        /// <summary>
        /// Adds a scheduled task on the server
        /// </summary>
        /// <param name="task">Implementation of the scheduled task class</param>
        protected void AddScheduledTask(ScheduledTask task)
        {
            IScheduledTaskManager manager = Services.GetService<IScheduledTaskManager>();
            manager.AddScheduledTask(task);
        }
    }
}
