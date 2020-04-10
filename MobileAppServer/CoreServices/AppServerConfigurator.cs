﻿using MobileAppServer.CoreServices.ControllerManagement;
using MobileAppServer.CoreServices.CoreServer;
using MobileAppServer.CoreServices.DIManagement;
using MobileAppServer.CoreServices.DomainModelsManagement;
using MobileAppServer.CoreServices.EFIManagement;
using MobileAppServer.CoreServices.InterceptorManagement;
using MobileAppServer.CoreServices.Logging;
using MobileAppServer.CoreServices.ProxyServices;
using MobileAppServer.CoreServices.ScheduledTaskManagement;
using MobileAppServer.CoreServices.SecurityManagement;
using MobileAppServer.CoreServices.TelemetryManagement;
using MobileAppServer.LoadBalancingServices;
using MobileAppServer.ManagedServices;
using MobileAppServer.ScheduledServices;
using MobileAppServer.Security;
using MobileAppServer.ServerObjects;
using System;
using System.Reflection;

namespace MobileAppServer.CoreServices
{
    public abstract class AppServerConfigurator
    {
        internal IServiceManager Services { get; set; }
        public AppServerConfigurator()
        {
            Services = ServiceManager.GetInstance();

            Services.Bind<ILoggingService>(typeof(LoggingServiceImpl), true);
            Services.Bind<IControllerManager>(typeof(ControllerManagerImpl), true);
            Services.Bind<IDomainModelsManager>(typeof(DomainModelsManager), true);
            Services.Bind<IEFIManager>(typeof(EFIManagerImpl), false);
            Services.Bind<ICoreServerService>("realserver", typeof(CoreServerImpl), true);
            Services.Bind<ICoreServerService>(typeof(ProxyCoreServer), true);
            Services.Bind<IHardwareServices>(typeof(HWServiceImpl), true);
            Services.Bind<IInterceptorManagerService>(typeof(InterceptorManagementServiceImpl), true);
            Services.Bind<IDependencyInjectionService>(typeof(DependencyInjectorManagerImpl), true);
            Services.Bind<ISecurityManagementService>(typeof(SecurityManagementServiceImpl), true);
            Services.Bind<IScheduledTaskManager>(typeof(ScheduledTaskManagerImpl), true);
            Services.Bind<IEncodingConverterService>(typeof(ServerEncodingConverterServiceImpl), false);
            Services.Bind<ITelemetryManagement>(typeof(TelemetryManagementImpl), true);
        }

        public abstract void ConfigureServices(IServiceManager serviceManager);
        public abstract ServerConfiguration GetServerConfiguration();


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