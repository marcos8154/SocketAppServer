using MobileAppServer.EFI;
using MobileAppServer.LoadBalancingServices;
using MobileAppServer.ScheduledServices;
using MobileAppServer.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MobileAppServer.ServerObjects
{
    public class Server
    {
        internal List<ControllerRegister> RegisteredControllers { get; set; }
        internal List<ModelRegister> RegisteredModels { get; set; }
        internal List<IHandlerInterceptor> Interceptors { get; private set; }
        internal List<IDependencyInjectorMaker> DependencyInjectorMakers { get; private set; }
        internal List<ScheduledTask> ScheduledTasks { get; private set; }
        internal List<IExtensibleFrameworkInterface> Extensions { get; private set; }


        public static Server GlobalInstance { get; private set; }

        private bool Started = false;

        public Socket ServerSocket { get; set; }

        private List<SocketSession> _sessions;

        private object lckSessions = new object();
        internal List<SocketSession> ClientSockets
        {
            get
            {
                lock (lckSessions)
                {
                    if (_sessions == null)
                        _sessions = new List<SocketSession>();
                    return _sessions;
                }
            }
        }

        public int BufferSize { get; set; }
        public int Port { get; set; }
        public int Requests { get; set; }
        public bool IsSingleThreaded { get; set; }
        public int MaxThreadsCount { get; set; }

        public int AttemptsToGetSubServerAvailable { get; private set; }

        public Server()
        {
            Interceptors = new List<IHandlerInterceptor>();
            DependencyInjectorMakers = new List<IDependencyInjectorMaker>();
        }

        public ILoggerWrapper Logger { get; private set; }

        public void SetDefaultLoggerWrapper(ILoggerWrapper wrapper)
        {
            Logger = wrapper;
        }

        public void EnableExtension(IExtensibleFrameworkInterface extension)
        {
            if (Extensions == null)
                Extensions = new List<IExtensibleFrameworkInterface>();
            Extensions.Add(extension);
        }

        public Encoding ServerEncoding { get; set; }

        private void Initialize()
        {
            GlobalInstance = this;
            Console.WriteLine("Socket App Server - version " + new ServerInfo().ServerVersion);

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (BufferSize == 0)
                BufferSize = 4096 * 10;

            if (ServerEncoding == null)
                ServerEncoding = Encoding.Default;

            if (MaxThreadsCount == 0)
                MaxThreadsCount = 999999;

            Requests = 0;
            if (Interceptors == null)
                Interceptors = new List<IHandlerInterceptor>();
            if (DependencyInjectorMakers == null)
                DependencyInjectorMakers = new List<IDependencyInjectorMaker>();

            RegisterController("ServerInfoController", typeof(ServerInfoController));

            Console.WriteLine($"Server started with {BufferSize} bytes for buffer size \n");
            Console.WriteLine($"Server Encoding: '{ServerEncoding.EncodingName}'");
            if (MaxThreadsCount > 0)
                Console.WriteLine($"Server max threads count: " + MaxThreadsCount);

            foreach(IExtensibleFrameworkInterface extension in Extensions)
            {
                try
                {
                    if (string.IsNullOrEmpty(extension.ExtensionName))
                        throw new Exception($"Cannot be load unknown extension from assembly '{extension.GetType().Assembly.FullName}'");
                    if (string.IsNullOrEmpty(extension.ExtensionVersion))
                        throw new Exception($"Cannot be read extension version for '{extension.ExtensionName}'");
                    if (string.IsNullOrEmpty(extension.ExtensionPublisher))
                        throw new Exception($"Cannot be load unknown publisher extension for '{extension.ExtensionName}'");

                    LogController.WriteLog($"Loading extension '{extension.ExtensionName}', version {extension.ExtensionVersion} by {extension.ExtensionPublisher}", ServerLogType.INFO);
                    extension.Load(this);
                    LogController.WriteLog($"Extension '{extension.ExtensionName}' successfully loaded");
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void RegisterInterceptor(IHandlerInterceptor interceptor)
        {
            Interceptors.Add(interceptor);
        }

        public void AddScheduledTask(ScheduledTask task)
        {
            if (string.IsNullOrEmpty(task.TaskName))
                throw new Exception("Task name is empty");
            if (task.Interval == null)
                throw new Exception("Task interval is null");

            if (ScheduledTasks == null)
                ScheduledTasks = new List<ScheduledTask>();

            ScheduledTasks.Add(task);
        }

        public IServerUserRepository UserRepository { get; private set; }
        public int TokenLifeTime { get; private set; }
        public string TokenCryptPassword { get; private set; }
        public void EnableSecurity(IServerUserRepository repository,
            int tokenLifetime = 3, string tokenCryptPassword = "")
        {
            TokenCryptPassword = tokenCryptPassword;
            UserRepository = repository;
            TokenLifeTime = tokenLifetime;

            RegisterController("AuthorizationController", typeof(AuthorizationController));

            List<IHandlerInterceptor> list = new List<IHandlerInterceptor>();
            if (Interceptors.Count > 0)
            {
                Interceptors.ForEach(i => list.Add(i));
                Interceptors.Clear();
            }

            RegisterInterceptor(new SecurityTokenInterceptor());
            RegisterInterceptor(new UserRoleValidationInterceptor());

            if (list.Count > 0)
                list.ForEach(i => RegisterInterceptor(i));
        }

        public LoadBalanceConfigurator EnableLoadBalanceServer(int maxAttemptsToGetAvailableSubServer = 3,
            bool cacheResultsForUnreachableServers = false)
        {
            EnableBasicServerController(new LoadBalanceServer());
            if (cacheResultsForUnreachableServers)
                LoadBalanceServer.EnableCachedResultsForUnreachableServers();

            AttemptsToGetSubServerAvailable = maxAttemptsToGetAvailableSubServer;
            return new LoadBalanceConfigurator();
        }

        public void RegisterDependencyInjectorMaker(IDependencyInjectorMaker injectorMaker)
        {
            if (injectorMaker == null)
                throw new Exception("Cannot be null");

            if (DependencyInjectorMakers == null)
                DependencyInjectorMakers = new List<IDependencyInjectorMaker>();
            DependencyInjectorMakers.Add(injectorMaker);
        }

        public void RegisterController(string name, Type type)
        {
            Console.WriteLine($"Registering controller '{name}'...");

            if (RegisteredControllers == null)
                RegisteredControllers = new List<ControllerRegister>();
            RegisteredControllers.Add(new ControllerRegister() { Name = name, Type = type });
        }

        public void RegisterAllControllers(Assembly assembly, string namespaceName)
        {
            Type[] controllers = GetTypesInNamespace(assembly, namespaceName);
            for (int i = 0; i < controllers.Length; i++)
                RegisterController(controllers[i].Name, controllers[i]);
        }

        public void RegisterAllModels(Assembly assembly, string namespaceName)
        {
            Type[] models = GetTypesInNamespace(assembly, namespaceName);
            for (int i = 0; i < models.Length; i++)
                RegisterModelType(models[i]);
        }

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        public void RegisterModelType(Type modelType)
        {
            if (RegisteredModels == null)
                RegisteredModels = new List<ModelRegister>();
            Console.WriteLine($"Registering model '{modelType.FullName}'");
            RegisteredModels.Add(new ModelRegister(modelType.FullName, modelType));
        }

        public delegate void StartServer();
        public event StartServer ServerStarted;

        public delegate void Reboot();
        public event Reboot RebootRequest;

        private void RunServerStartupTasks()
        {
            if (ScheduledTasks != null)
                ScheduledTasks
                    .Where(t => t.RunOnServerStart)
                    .ToList()
                    .ForEach(t =>
                    {
                        new ScheduledTaskExecutorService().Execute(t);
                    });
        }

        public Type BasicServerControllerType { get; private set; }
        public bool IsBasicServerControllerMode { get; private set; }

        public void EnableBasicServerController(IBasicServerController singleServerController)
        {
            BasicServerControllerType = singleServerController.GetType();
            IsBasicServerControllerMode = true;
        }

        public void Start(bool waitForUserTypeExit = true)
        {
            if (Started)
                throw new Exception("Server already has been started.");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Initialize();
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));

            ServerSocket.Listen(0);
            ServerSocket.BeginAccept(AcceptCallback, null);
            Started = true;
            ServerStarted?.Invoke();
            RunServerStartupTasks();
            sw.Stop();
            LogController.WriteLog(new ServerLog($"Server started in {sw.ElapsedMilliseconds}ms", "Server", "Start"));
            LogController.WriteLog(new ServerLog($"Running at port {Port}"));
            Console.WriteLine("Type 'exit' to stop; 'reboot' to send reboot request event...");
            string line = "";

            if (waitForUserTypeExit)
                while (line != "exit")
                {
                    line = Console.ReadLine();
                    if (line == "reboot")
                        RebootRequest?.Invoke();
                }
        }

        public void SendReboot()
        {
            while (RequestProccess.ThreadCount > 1)
            {
                Thread.Sleep(300);
                //wait...
            }

            ServerSocket.Close();
            ServerSocket.Dispose();
            ServerSocket = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            ServerSocket.Listen(0);
            ServerSocket.BeginAccept(AcceptCallback, null);

            RebootRequest?.Invoke();
        }

        private object lockAccept = new object();
        private void AcceptCallback(IAsyncResult AR)
        {
            lock (lockAccept)
            {
                Socket socket;

                try
                {
                    socket = ServerSocket.EndAccept(AR);
                    var session = new SocketSession(socket, BufferSize);

                    ClientSockets.Add(session);

                    socket.BeginReceive(session.SessionStorage, 0, BufferSize, SocketFlags.None, ReceiveCallback, socket);
                    ServerSocket.BeginAccept(AcceptCallback, null);
                }
                catch (Exception ex) // I cannot seem to avoid this (on exit when properly closing sockets)
                {
                    LogController.WriteLog(new ServerLog("*** ERROR ***: \n" + ex.Message, ServerLogType.ERROR));
                }
            }
        }

        private object lockReceive = new object();
        private void ReceiveCallback(IAsyncResult AR)
        {
            lock (lockReceive)
            {
                new RequestPreProcess(AR);
            }
        }
    }
}
