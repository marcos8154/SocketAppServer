using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.LoadBalancingServices
{
    public  class LoadBalanceConfigurator
    {
        public LoadBalanceConfigurator EnableSubServerAutoRequirement(INotifiableSubServerRequirement notifiable,
            int serversLifetimeInMinutes = 10)
        {
            LoadBalanceServer.EnableSubServerAutoRequirement(notifiable, serversLifetimeInMinutes);
            return this;
        }

        public LoadBalanceConfigurator AddSubServer(string address, int port, Encoding encoding,
            int bufferSize, int maxConnectionAttempts,
            int acceptableProcesses)
        {
            LoadBalanceServer.AddSubServer(address, port,
                encoding, bufferSize, maxConnectionAttempts, acceptableProcesses);
            return this;
        }
    }
}
