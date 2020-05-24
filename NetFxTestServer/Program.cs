
using BalcaoMobileApp.ViewModels;
using SocketAppServerClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFxTestServer
{
    class Program
    {
        static void Handle()
        {
            Client c = new Client();
            RequestBody rb = RequestBody.Create("ProdutoController", "ObterGrupos");
            c.SendRequest(rb);
            List<GrupoViewModel> grupos = c.GetResult<List<GrupoViewModel>>();

            if (grupos.Count > 0)
                Console.WriteLine("Deu certo");
            Console.ReadKey();
            Handle();
        }

        static void Main(string[] args)
        {
            Client.Configure("localhost", 4050, Encoding.UTF8, 1000000);
            Handle();
            /*
            Console.ForegroundColor = ConsoleColor.White;
            SocketServerHost.CreateHostBuilder()
                   .UseStartup<Startup>()
                   .Run();
                   */
        }
        /*
        public class Startup : AppServerConfigurator
        {
            public override void ConfigureServices(IServiceManager serviceManager)
            {
                DisableStatisticsComputing();
                DisableTelemetryServices();

                RegisterController(typeof(ImageController));
            }

            public override ServerConfiguration GetServerConfiguration()
            {
                //Here, we must return the object that contains
                //the server's operating parameters, such as port, 
                //Encoding, buffer and connection limit
                return new ServerConfiguration(Encoding.UTF8,
                         7000, 10000000, false, 100, true);
            }
        }
  */
    }
}
