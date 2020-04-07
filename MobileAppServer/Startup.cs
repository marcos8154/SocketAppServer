using MobileAppServer.CoreServices;
using MobileAppServer.CoreServices.CoreServer;
using MobileAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer
{
    public class Startup : AppServerConfigurator
    {
        public override void ConfigureServices(IServiceManager serviceManager)
        {
            RegisterController(typeof(TestController));
            RegisterModel(typeof(Entity));
        }

        public override ServerConfiguration GetServerConfiguration()
        {
            return new ServerConfiguration(Encoding.UTF8, 4000);
        }
    }
}
