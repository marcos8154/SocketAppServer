using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.ServerObjects
{
    /// <summary>
    /// Prevents the action from receiving simultaneous requests
    /// It is possible to customize the behavior through the parameters of this attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SingleThreaded : Attribute
    {
        /// <summary>
        /// Maximum number of waiting attempts to release the action
        /// Default value = 10
        /// </summary>
        public int WaitingAttempts { get; set; }

        /// <summary>
        /// Waiting time between each attempt (in miliseconds)
        /// Default value = 100
        /// </summary>
        public int WaitingInterval { get; set; }

        public SingleThreaded()
        {
            WaitingAttempts = 10;
            WaitingInterval = 100;
        }
    }
}
