using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.Hosting
{
    public interface ISocketServerHostBuilder
    {
        ISocketServerHostBuilder UseStartup<TStartup>() where TStartup : class;
        void Run();
    }
}
