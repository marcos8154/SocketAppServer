using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CLIHost;
using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnAnyExtensionFromFile
{
    public class FunnyCommand : ICLIClient
    {
        public void Activate()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            ILoggingService logging = manager.GetService<ILoggingService>();
            logging.WriteLog("Hi! Im a simple CLI function running inside this beautiful extension loaded dinamically! :)");
        }
    }
}
