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
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.LoadBalancingServices;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.CoreServer
{
    internal class CoreServerImpl : ICoreServerService
    {
        private object lockAccept = new object();
        private object lockReceive = new object();
        private object lockRemoveSession = new object();
        private object lckSession = new object();

        private bool isBasicServerEnabled;
        private bool isLoadBalanceEnabled;
        private bool telemetryServicesDisabled;

        private IServiceManager serviceManager;
        private ILoggingService loggingService;
        private ISecurityManagementService security;
        private ICLIHostService cliHost;
        private IEncodingConverterService encodingService;
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
                try
                {
                    Socket socket = serverSocket.EndAccept(AR);
                    var session = new SocketSession(socket, configuration.BufferSize, AR);

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

        public void ReceiveCallback(IAsyncResult AR)
        {
            lock (lockReceive)
            {
                RequestPreProcessor preProcessor = new RequestPreProcessor(AR);
                preProcessor.Process();
            }
        }

        public void RemoveSession(ref SocketSession session)
        {
            lock (lockRemoveSession)
            {
                clientSessions.Remove(session);
                if (session != null)
                {
                    //         session.Clear();
                    session.Close();
                    session = null;
                }
            }
        }

        public int CurrentThreadsCount()
        {
            return RequestProcessor.ThreadCount;
        }

        public IReadOnlyCollection<SocketSession> GetCurrentSessions()
        {
            return clientSessions.ToList().AsReadOnly();
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

            if (!CSL.ConsoleDisabled) Console.WriteLine("Type 'exit' to stop; 'reboot' to send reboot request event...");
            string line = "";

            if (!CSL.ConsoleDisabled)
            {
                cliHost = serviceManager.GetService<ICLIHostService>();
                if (cliHost.RegisteredCommands().Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\nCommands available on this CLI:");
                    foreach (CLICommand command in cliHost.RegisteredCommands())
                        Console.WriteLine($"=>  {command.CommandText}: {command.CommandDescription}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            if (configuration.IsConsoleApplication)
                while (line != "exit")
                {
                    line = Console.ReadLine();

                    CLICommand command = cliHost.GetCommand(line);
                    if (command == null)
                        Console.WriteLine("No such command");
                    else
                        TryActivateCommand(command);

                    if (line == "reboot")
                        Reboot();
                }
        }

        private void TryActivateCommand(CLICommand command)
        {
            cliHost.SetCLIBusy(true);
            try
            {
                command.ExecutorClient.Activate();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += $@"n\{ex.InnerException.Message}";
                loggingService.WriteLog($"Command '{command.CommandText}' failed to activate. Exception: {msg}", ServerLogType.ERROR);
            }
            cliHost.SetCLIBusy(false);
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
            encodingService = serviceManager.GetService<IEncodingConverterService>();
            security = serviceManager.GetService<ISecurityManagementService>();

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IControllerManager manager = serviceManager.GetService<IControllerManager>();
            manager.RegisterController(typeof(ServerInfoController));

            if (telemetryServicesDisabled)
            {
                string alert = "WARNING!: Disabling telemetry services can bring some extra performance to the server (even if perhaps imperceptible). However it will not be possible to collect metrics to implement improvements in your code";
                if (!CSL.ConsoleDisabled) Console.ForegroundColor = ConsoleColor.Yellow;
                if (!CSL.ConsoleDisabled) Console.WriteLine(alert);
                if (!CSL.ConsoleDisabled) Console.ForegroundColor = ConsoleColor.White;

                serviceManager.Unbind<ITelemetryManagement>();
                loggingService.WriteLog(alert, ServerLogType.ALERT);
            }
            else
            {
                ITelemetryManagement telemetry = serviceManager.GetService<ITelemetryManagement>();
                telemetry.Initialize();
            }

            IEFIManager efiManager = serviceManager.GetService<IEFIManager>();
            efiManager.LoadAll();

            if (_basicProcessorType != null)
                EnableBasicServerProcessorModeInternal();

            try
            {
                if (!CSL.ConsoleDisabled) Console.WriteLine("Socket App Server - version " + new ServerInfo().ServerVersion);
            }
            catch { }

            if (!CSL.ConsoleDisabled) Console.WriteLine($"Server started with {configuration.BufferSize} bytes for buffer size \n");
            if (!CSL.ConsoleDisabled) Console.WriteLine($"Server Encoding: '{configuration.ServerEncoding.EncodingName}'");
            if (configuration.MaxThreadsCount > 0)
                if (!CSL.ConsoleDisabled) Console.WriteLine($"Server max threads count: " + configuration.MaxThreadsCount);
        }

        private Type _basicProcessorType = null;
        public void EnableBasicServerProcessorMode(Type basicProcessorType)
        {
            _basicProcessorType = basicProcessorType;
        }

        private void EnableBasicServerProcessorModeInternal()
        {
            if (_basicProcessorType == typeof(LoadBalanceServer))
            {
                if (security.IsAuthenticationEnabled())
                    throw new InvalidOperationException("Load balancing cannot be enabled when authentication services are enabled");
                isLoadBalanceEnabled = (_basicProcessorType == typeof(LoadBalanceServer));
            }

            serviceManager.Bind<IBasicServerController>(_basicProcessorType, false);
            isBasicServerEnabled = true;
        }

        public bool IsBasicServerEnabled()
        {
            return isBasicServerEnabled;
        }

        public SocketSession GetSession(Socket clientSocket)
        {
            lock (lckSession)
            {
                try
                {
                    return GetCurrentSessions().FirstOrDefault(s => s.ClientSocket.Equals(clientSocket));
                }
                catch
                {
                    return GetSession(clientSocket);
                }
            }
        }



        public bool IsLoadBalanceEnabled()
        {
            return isLoadBalanceEnabled;
        }

        public string GetServerVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }


        public void DisableTelemetryServices()
        {
            telemetryServicesDisabled = true;
        }

        public void Stop()
        {
            try
            {
                serverSocket.Close();
                serverSocket.Dispose();
                serverSocket = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch { }
        }
    }
}
