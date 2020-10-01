/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.LoadBalancingServices
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
        /// <param name="maxConnectionAttempts">Maximum number of attempts to connect to the sub-server</param>
        /// <param name="acceptableProcesses">Number of simultaneous requests supported by the sub-server. If the limit is met, the balancer will find another server in the pool</param>
        /// <returns></returns>
        public LoadBalanceConfigurator AddSubServer(string address, 
            int port, Encoding encoding,
            int maxConnectionAttempts,
            int acceptableProcesses)
        {
            LoadBalanceServer.AddSubServer(address, port,
                encoding, maxConnectionAttempts, acceptableProcesses);
            return this;
        }
    }
}
