using SocketAppServer;
using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.ManagedServices;
using SocketAppServer.Security;
using SocketAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFxTestServer
{
    public class Startup : AppServerConfigurator
    {
        public override void ConfigureServices(IServiceManager serviceManager)
        {
            RegisterController(typeof(C1));
        }

        public override ServerConfiguration GetServerConfiguration()
        {
            return new ServerConfiguration(Encoding.UTF8, 1001);
        }
    }

    public class C1 : IController
    {
        [ServerAction]
        public string Hello(SocketRequest request)
        {
            return "Ok;";
        }
    }
}
