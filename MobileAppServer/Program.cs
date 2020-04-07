using MobileAppServer.Hosting;
using MobileAppServer.LoadBalancingServices;
using MobileAppServer.ServerObjects;
using System;
using System.Text;

namespace MobileAppServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SocketServerHost.CreateHostBuilder()
                .UseStartup<Startup>()
                .Run();
        }
    }
}
