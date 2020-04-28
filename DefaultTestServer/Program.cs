using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.Extensions.ClientMaker;
using SocketAppServer.Hosting;
using SocketAppServer.ManagedServices;
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
            string[] alphabet = File.ReadAllLines(@"C:\temp\alphabet.txt");

            foreach (string letter in alphabet)
            {
                for (int i = 0; i < 100; i++)
                {
                    string key = $"{letter}-{i + 1}";
                    int value = new Random(i).Next();
                    CacheRepository<int>.Set(key, value, 60);
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Cache<int> cached = CacheRepository<int>.Get(k);
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            CacheRepository<int>.ExpireAll("Z-1");
            /*
             *                 SocketServerHost.CreateHostBuilder()
                    .UseStartup<Startup>()
                    .Run();
             */

            Console.ReadKey();
        }

        public class Startup : AppServerConfigurator
        {
            public override void ConfigureServices(IServiceManager serviceManager)
            {
                RegisterController(typeof(DeviceController));
                EnableExtension(new SocketClientLayerGenerator());
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
