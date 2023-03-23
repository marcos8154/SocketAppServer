
using System;
using System.Text;
using System.Web.Http;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;

namespace SocketAppServer.Extensions.HttpServer
{
    public class MockSrv: AppServerConfigurator
    {
        public override void ConfigureServices(IServiceManager serviceManager)
        {
            EnableExtension(new HttpModule("http://localhost:6500/"));
          //  RegisterController(typeof(TesteController));
        }

        public override ServerConfiguration GetServerConfiguration()
        {
            return new ServerConfiguration(
                    serverEncoding: Encoding.UTF8,
                    port: 6000
                );
        }

    }

    /*
    [Route("api/{controller}")]
    public class TesteController : ApiController, IController
    {
        [HttpGet()]
        [Route("hello")]
        [ServerAction]
        public string Hello()
        {
            return "Funcionando";
        }
    }
    */

    public class Program
    {
        public static void Main(string[] args)
        {
            SocketServerHost.CreateHostBuilder()
                .UseStartup<MockSrv>()
                .Run();
        }

  
    
    }
}
