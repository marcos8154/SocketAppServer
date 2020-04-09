using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.ManagedServices
{
    public sealed class ServiceManager
    {
        private static IServiceManager _instance = null;
        public static IServiceManager GetInstance()
        {
            if (_instance == null)
                _instance = new ServiceManagerImpl();
            return _instance;
        }
    }
}
