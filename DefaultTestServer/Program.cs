using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.Extensions.ClientMaker;
using SocketAppServer.Hosting;
using SocketAppServer.ManagedServices;
using SocketAppServer.Security;
using SocketAppServer.ServerObjects;
using SocketAppServer.TelemetryServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public class Startup : AppServerConfigurator
        {
            public override void ConfigureServices(IServiceManager serviceManager)
            {
                RegisterController(typeof(DeviceController));
                EnableExtension(new SocketClientLayerGenerator());
                UseAuthentication(new UserRepos());
            }

            public override ServerConfiguration GetServerConfiguration()
            {
                return new ServerConfiguration(Encoding.UTF8,
                         5000, 1024 * 100, false, 100, true);
            }
        }

        public class UserRepos : IServerUserRepository
        {
            public ServerUser Authenticate(string userNameOrEmail, string password)
            {
                return new ServerUser("1235", "UserName", "email@provider.com", "Organization Test Name");
            }
        }

        public class DeviceController : IController
        {
            [ServerAction(ExceptionHandler = typeof(MySimpleExceptionHandler))]
            public void RegisterDevice(string deviceName,
                SocketRequest request)
            {
                ILoggingService log = ServiceManager.GetInstance().GetService<ILoggingService>();
                log.WriteLog("DISPOSITIVO REGISTRADO COM SUCESSO");
                //   return "Dispositivo registrado com sucesso";

            }

            [ServerAction]
            public List<string> GetRetistered(bool all, List<string> excludeList, Int32 countLimit)
            {
                return new List<string>();
            }
        }
    }
}
