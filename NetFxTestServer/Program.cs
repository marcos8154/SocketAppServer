
using BalcaoMobileApp.ViewModels;
using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.Hosting;
using SocketAppServer.LoadBalancingServices;
using SocketAppServer.ManagedServices;
using SocketAppServerClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NetFxTestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Client.Configure("localhost", 4050, Encoding.UTF8, 1000000);
            Handle();
            */
            Console.ForegroundColor = ConsoleColor.White;
            SocketServerHost.CreateHostBuilder()
                   .UseStartup<Startup>()
                   .Run();
        }

        public class LoggerWrapper : ILoggerWrapper
        {
            public void Register(ref ServerLog log)
            {
                //   Console.WriteLine(log.LogText);
            }

            public List<ServerLog> List(Expression<Func<ServerLog, bool>> query)
            {
                throw new NotImplementedException();
            }
        }

        private class SubServerAllocationManager : INotifiableSubServerRequirement
        {
            private static List<PortInfo> availablePorts;
            public SubServerAllocationManager()
            {
                if (availablePorts == null)
                {
                    availablePorts = new List<PortInfo>()
                    {
                        new PortInfo(6001),
                        new PortInfo(6002),
                        new PortInfo(6003),
                        new PortInfo(6005)
                    };
                }
            }

            private static object lckAlloc = new object();
            private static object lckDealloc = new object();

            public SubServer StartNewInstance()
            {
                lock (lckAlloc)
                {
                    for (int i = 0; i < availablePorts.Count; i++)
                    {
                        if (availablePorts[i].IsAvailable)
                        {
                            string path = @"C:\Users\Marcos Vinícius\Documents\Visual Studio 2015\Projects\MobileAppServer\DefaultTestServer\bin\Debug\netcoreapp3.1";
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WorkingDirectory = path;
                            startInfo.Arguments = availablePorts[i].PortNumber.ToString();
                            startInfo.FileName = $@"{path}\DefaultTestServer.exe";
                            startInfo.CreateNoWindow = true;
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.UseShellExecute = false;

                            Process p = new Process();
                            p.StartInfo = startInfo;
                        
                            availablePorts[i].SetServerProccess(p);
                            availablePorts[i].ChangeAvailable();

                            return new SubServer("localhost", availablePorts[i].PortNumber, Encoding.UTF8, 1, 4);
                        }
                    }

                    return null;
                }
            }

            public void StopInstance(SubServer server)
            {
                lock (lckDealloc)
                {
                    PortInfo port = availablePorts.FirstOrDefault(p =>
                        p.PortNumber == server.Port);
                    if (port != null)
                        port.ChangeAvailable();
                }
            }
        }


        public class Startup : AppServerConfigurator
        {
            public override void ConfigureServices(IServiceManager serviceManager)
            {
                DisableStatisticsComputing();
                DisableTelemetryServices();

                EnableFromDiskExtension(@"C:\Users\Marcos Vinícius\Documents\Visual Studio 2015\Projects\MobileAppServer\AnAnyExtensionFromFile\bin\Debug\AnAnyExtensionFromFile.dll");
                serviceManager.GetService<ILoggingService>().SetWrapper(new LoggerWrapper());

                RegisterController(typeof(ImageController));

                EnableLoadBalanceServer()
                    .AddSubServer("localhost", 6000, Encoding.UTF8, 1, 5)
                    .EnableDynamicInstanceAllocationManagement(new SubServerAllocationManager(), 1);
            }

            public override ServerConfiguration GetServerConfiguration()
            {
                //Here, we must return the object that contains
                //the server's operating parameters, such as port, 
                //Encoding, buffer and connection limit
                return new ServerConfiguration(Encoding.UTF8,
                         7001, 10000000, false, 100, true);
            }
        }
    }
}
