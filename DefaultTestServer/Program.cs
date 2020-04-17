using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.Hosting;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using System;
using System.Text;

namespace DefaultTestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServerHost.CreateHostBuilder()
                .UseStartup<Startup>()
                .Run();
        }
    }

    public class Startup : AppServerConfigurator
    {
        public override void ConfigureServices(IServiceManager serviceManager)
        {
            RegisterController(typeof(DeviceController));
        }

        public override ServerConfiguration GetServerConfiguration()
        {
            return new ServerConfiguration(Encoding.UTF8,
                     5000, 1024 * 100, false, 100, true);
        }
    }

    public class DeviceController : IController
    {
        public ActionResult RegisterDevice(string deviceName,
            SocketRequest request )
        {
            return ActionResult.Json(new OperationResult(true, 600, ""));
        }
    }
}
