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

using MobileAppServer.CoreServices.Logging;
using MobileAppServer.LoadBalancingServices;
using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.CoreServer
{
    internal class CoreServerImpl : ICoreServerService
    {
        private object lockAccept = new object();
        private object lockReceive = new object();
        private object lockRemoveSession = new object();

        private bool isBasicServerEnabled;
        private bool isLoadBalanceEnabled;

        private IServiceManager serviceManager;
        private ILoggingService loggingService;
        private ISecurityManagementService security; 
        private Socket serverSocket;
        private ServerConfiguration configuration;
        private List<SocketSession> clientSessions;

        public CoreServerImpl()
        {
            clientSessions = new List<SocketSession>();
       
        }

        public ServerConfiguration GetConfiguration()
        {
            return configuration;
        }

        public void AcceptCallback(IAsyncResult AR)
        {
            lock (lockAccept)
            {
                Socket socket;

                try
                {
                    socket = serverSocket.EndAccept(AR);
                    var session = new SocketSession(socket, configuration.BufferSize);

                    clientSessions.Add(session);
                    socket.BeginReceive(session.SessionStorage, 0, configuration.BufferSize, SocketFlags.None, ReceiveCallback, socket);
                    serverSocket.BeginAccept(AcceptCallback, null);
                }
                catch (Exception ex) // I cannot seem to avoid this (on exit when properly closing sockets)
                {
                    loggingService.WriteLog("*** ERROR ***: \n" + ex.Message, ServerLogType.ERROR);
                }
            }
        }

        public int CurrentThreadsCount()
        {
            return RequestProcessor.ThreadCount;
        }

        public IReadOnlyCollection<SocketSession> GetCurrentSessions()
        {
            return clientSessions.AsReadOnly();
        }

        public bool IsServerStarted()
        {
            return serverSocket != null;
        }

        public void Reboot()
        {
            while (RequestProcessor.ThreadCount > 1)
            {
                Thread.Sleep(300);
                //wait...
            }

            serverSocket.Close();
            serverSocket.Dispose();
            serverSocket = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, configuration.Port));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        public void ReceiveCallback(IAsyncResult AR)
        {
            lock (lockReceive)
                new RequestPreProcessor(AR);
        }

        public void SetConfiguration(ServerConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Start()
        {
            if (serverSocket != null)
                throw new Exception("Server already has been started.");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Initialize();

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, configuration.Port));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);

            RunServerStartupTasks();
            sw.Stop();

            loggingService.WriteLog($"Server started in {sw.ElapsedMilliseconds}ms", "Server", "Start");
            loggingService.WriteLog($"Running at port {configuration.Port}");

            Console.WriteLine("Type 'exit' to stop; 'reboot' to send reboot request event...");
            string line = "";

            if (configuration.IsConsoleApplication)
                while (line != "exit")
                {
                    line = Console.ReadLine();
                    if (line == "reboot")
                        Reboot();
                }
        }

        public void RunServerStartupTasks()
        {
            IScheduledTaskManager manager = serviceManager.GetService<IScheduledTaskManager>();
            manager.RunServerStartupTasks();
        }

        private void Initialize()
        {
            serviceManager = ServiceManager.GetInstance();
            loggingService = serviceManager.GetService<ILoggingService>();
            security = serviceManager.GetService<ISecurityManagementService>();

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IControllerManager manager = serviceManager.GetService<IControllerManager>();
            manager.RegisterController(typeof(ServerInfoController));

            ITelemetryManagement telemetry = serviceManager.GetService<ITelemetryManagement>();
            telemetry.Initialize();

            Console.WriteLine("Socket App Server - version " + new ServerInfo().ServerVersion);
            Console.WriteLine($"Server started with {configuration.BufferSize} bytes for buffer size \n");
            Console.WriteLine($"Server Encoding: '{configuration.ServerEncoding.EncodingName}'");
            if (configuration.MaxThreadsCount > 0)
                Console.WriteLine($"Server max threads count: " + configuration.MaxThreadsCount);
        }

        public void EnableBasicServerProcessorMode(Type basicProcessorType)
        {
            if (basicProcessorType == typeof(LoadBalanceServer))
                if (security.IsAuthenticationEnabled())
                    throw new InvalidOperationException("Load balancing cannot be enabled when authentication services are enabled");

            serviceManager.Bind<IBasicServerController>(basicProcessorType, false);
            isBasicServerEnabled = true;
            isLoadBalanceEnabled = (basicProcessorType == typeof(LoadBalanceServer));
        }

        public bool IsBasicServerEnabled()
        {
            return isBasicServerEnabled;
        }

        public SocketSession GetSession(Socket clientSocket)
        {
            return GetCurrentSessions().FirstOrDefault(s => s.ClientSocket.Equals(clientSocket));
        }

        public void RemoveSession(SocketSession session)
        {
            lock (lockRemoveSession)
            {
                clientSessions.Remove(session);
                session.Close();
                session = null;
            }
        }

        public bool IsLoadBalanceEnabled()
        {
            return isLoadBalanceEnabled;
        }
    }
}
