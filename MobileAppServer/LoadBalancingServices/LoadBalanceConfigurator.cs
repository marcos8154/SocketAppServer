using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.LoadBalancingServices
{
    /// <summary>
    /// Load balancing service configurator for the current server
    /// </summary>
    public class LoadBalanceConfigurator
    {
        /// <summary>
        /// Enables the request for dynamic allocation of new instances of sub-servers to the pool
        /// </summary>
        /// <param name="notifiable">Implementation of a notifying interface for the acquisition and shutdown of instances dynamically allocated to the sub-server pool</param>
        /// <param name="serversLifetimeInMinutes">Lifetime (in minutes) that the sub-server instance should remain in the pool. At the end of the lifetime, the server will be detached from the pool and the INotifiableSubServerRequirement class will be notified to shut down the sub-server</param>
        /// <returns></returns>
        public LoadBalanceConfigurator EnableDynamicInstanceAllocationManagement(INotifiableSubServerRequirement notifiable,
            int serversLifetimeInMinutes = 10)
        {
            LoadBalanceServer.EnableSubServerAutoRequirement(notifiable, serversLifetimeInMinutes);
            return this;
        }

        /// <summary>
        /// Add a sub server to the load balancer target server pool
        /// </summary>
        /// <param name="address"Network path of the destination server></param>
        /// <param name="port">Sub server port</param>
        /// <param name="encoding">Sub server encoding (Must match the same Encoding configured on the destination server)</param>
        /// <param name="bufferSize">Subserver buffer size</param>
        /// <param name="maxConnectionAttempts">Maximum number of attempts to connect to the sub-server</param>
        /// <param name="acceptableProcesses">Number of simultaneous requests supported by the sub-server. If the limit is met, the balancer will find another server in the pool</param>
        /// <returns></returns>
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
