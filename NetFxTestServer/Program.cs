
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
    }
}
