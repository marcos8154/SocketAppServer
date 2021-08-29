using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.ManagedServices;
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
            ICoreServerService coreServer = serviceManager.GetService<ICoreServerService>("realserver");
            coreServer.EnableBasicServerProcessorMode(typeof(BasicServer));
        }

        public override ServerConfiguration GetServerConfiguration()
        {
            return new ServerConfiguration(Encoding.ASCII, 1001);
        }
    }
}
