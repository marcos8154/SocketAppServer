using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Text;
using static DefaultTestServer.Program;

namespace DefaultTestServer
{

    public class Startup : AppServerConfigurator
    {
        public static int serverPort;

        public override void ConfigureServices(IServiceManager serviceManager)
        {
            DisableStatisticsComputing();
            //   DisableTelemetryServices();
            RegisterController(typeof(CustomerController));
            RegisterModel(typeof(Customer));
            RegisterModel(typeof(Info));

            UseAuthentication(new UserRepository());
        }

        public override ServerConfiguration GetServerConfiguration()
        {
            //Here, we must return the object that contains
            //the server's operating parameters, such as port, 
            //Encoding, buffer and connection limit
            return new ServerConfiguration(Encoding.UTF8,
                     6500, 10000000, false, 100, true);
        }
    }
}
