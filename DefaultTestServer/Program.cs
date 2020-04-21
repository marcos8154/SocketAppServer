using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.Hosting;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using System;
using System.Text;
using System.Threading;

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
            RegisterCLICommand("run", "Allows run a simple command", new MySimpleCLICommand());
        }

        public override ServerConfiguration GetServerConfiguration()
        {
            return new ServerConfiguration(Encoding.UTF8,
                     5000, 1024 * 100, false, 100, true);
        }
    }

    public class DeviceController : IController
    {
        [ServerAction(ExceptionHandler = typeof(MySimpleExceptionHandler))]
        public void RegisterDevice(string deviceName,
            SocketRequest request)
        {
            throw new Exception("Generic Error");
            ILoggingService log = ServiceManager.GetInstance().GetService<ILoggingService>();
            log.WriteLog("DISPOSITIVO REGISTRADO COM SUCESSO");
            //   return "Dispositivo registrado com sucesso";
          
        }
    }
}
