using System;
using System.Collections.Generic;
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
        internal static List<ControllerRegister> RegisteredControllers { get; set; }
        internal static List<ModelRegister> RegisteredModels { get; set; }

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
        public int Port = 14555;
        public int Requests { get; set; }
        public bool IsSingleThreaded { get; set; }
        public int MaxThreadsCount { get; set; }

        public Encoding ServerEncoding { get; set; }

        private void Initialize()
        {
            Console.WriteLine("Socket App Server 1.2.10");

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (BufferSize == 0)
                BufferSize = 4096 * 10;

            if (ServerEncoding == null)
                ServerEncoding = Encoding.Default;

            if (MaxThreadsCount == 0)
                MaxThreadsCount = 999999;

            Requests = 0;
            RegisterController("ServerInfoController", typeof(ServerInfoController));
            GlobalInstance = this;

            Console.WriteLine($"Server started with {BufferSize} bytes for buffer size \n");
            Console.WriteLine($"Server Encoding: '{ServerEncoding.EncodingName}'");
            if (MaxThreadsCount > 0)
                Console.WriteLine($"Server max threads count: " + MaxThreadsCount);
        }

        public void RegisterController(string name, Type type)
        {
            Console.WriteLine("\nRegistering a new controller...");

            if (RegisteredControllers == null)
                RegisteredControllers = new List<ControllerRegister>();
            RegisteredControllers.Add(new ControllerRegister() { Name = name, Type = type });

            Console.WriteLine($@"
*************************************
Controller registration info: 
Name: {name}
Type: {type.FullName}
*************************************");
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
            Console.WriteLine("Registering model...");
            RegisteredModels.Add(new ModelRegister(modelType.FullName, modelType));
        }

        public void Start(bool waitForUserTypeExit = true)
        {
            if (Started)
                throw new Exception("Server already has been started.");

            Initialize();
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            ServerSocket.Listen(0);
            ServerSocket.BeginAccept(AcceptCallback, null);
            Started = true;
            
            Console.WriteLine("Type 'exit' to stop...");
            string line = "";

            if (waitForUserTypeExit)
                while (line != "exit")
                {
                    line = Console.ReadLine();
                }
        }

        public void SendReboot()
        {
            while(RequestProccess.ThreadCount > 1)
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
                    LogController.WriteLog("*** ERROR ***: \n" + ex.Message);
                }
            }
        }

        private object lockReceive = new object();
        public void ReceiveCallback(IAsyncResult AR)
        {
            lock (lockReceive)
            {
                new RequestPreProcess(AR);
            }
        }
    }

    public class RequestPreProcess
    {
        private SocketSession GetSession(Socket clientSocket)
        {
            try
            {
                var session = Server.GlobalInstance.ClientSockets.FirstOrDefault(c => c.ClientSocket.Equals(clientSocket));
                return session;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public RequestPreProcess(IAsyncResult AR)
        {
            Socket clientSocket = null;

            clientSocket = (Socket)AR.AsyncState;
            int received;

            SocketSession session = null;

            for (int i = 0; i < 3; i++)
            {
                session = GetSession(clientSocket);
                if (session != null)
                    break;
                else Thread.Sleep(100);
            }

            if (session == null)
                return;

            try
            {
                received = clientSocket.EndReceive(AR);
            }
            catch (Exception ex)
            {
                // Don't shutdown because the socket may be disposed and its disconnected anyway.

                try
                {
                    //   clientSocket.Close();
                    session.Close();
                    Server.GlobalInstance.ClientSockets.Remove(session);
                }
                catch { }
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(session.SessionStorage, recBuf, received);
            string uriRequest = Server.GlobalInstance.ServerEncoding.GetString(recBuf);
            string resultText = string.Empty;

            RequestProccess process = new RequestProccess(uriRequest, clientSocket);

            if (Server.GlobalInstance.IsSingleThreaded)
                process.DoInBackGround(0);
            else
            {
                while (Server.GlobalInstance.MaxThreadsCount > 0 &&
                   RequestProccess.ThreadCount >= Server.GlobalInstance.MaxThreadsCount)
                {
                    LogController.WriteLog("\nNumber of current threads has exceeded the set limit. Waiting for tasks to finish...");
                    Thread.Sleep(300);
                }

                process.Execute(0);
            }
        }
    }
}
