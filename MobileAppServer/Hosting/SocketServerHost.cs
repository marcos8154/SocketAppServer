using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.Hosting
{
    public class SocketServerHost
    {
        public static ISocketServerHostBuilder CreateHostBuilder()
        {
            return new SocketHostBuilderImpl();
        }
    }
}
