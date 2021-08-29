using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.ServerObjects
{
    /// <summary>
    /// High-level and secure signals that change the behavior of the Thread-Executor that is managing this call
    /// </summary>
    public enum EngineFlags
    {
        /// <summary>
        /// Prevent the Thread-Executor from being Disposed immediately after this call (interrupting and destroying all objects created on the Thread). You must provide a method called 
        /// <br/> <nbsp/><nbsp/> <nbsp/>"public virtual void ReceiveDisposePending(IDisposePendingObject obj)" 
        /// <br/> so that the kernel transfers to your object the responsibility of invoking the dispose target internal object manually
        /// </summary>
        PREVENT_DISPOSE = 0x105001,

    }

    public class SinalizeAttribute : Attribute
    {
  
    }
}
