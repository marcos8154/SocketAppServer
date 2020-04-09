using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    /// <summary>
    /// Obtains basic hardware information about the current server process
    /// </summary>
    public interface IHardwareServices
    {
        /// <summary>
        /// Gets the average % CPU usage in the last X hours
        /// </summary>
        /// <param name="lastHours"></param>
        /// <returns></returns>
        double AverageCPUUsage(int lastHours = 3);

        /// <summary>
        /// Gets the average MB memory usage in the last X hours
        /// </summary>
        /// <param name="lastHours"></param>
        /// <returns></returns>
        double AverageMemoryUsage(int lastHours = 3);
       
        /// <summary>
        /// Waits for current processes to end on the server to force 
        /// immediate freeing of memory in the current server process
        /// </summary>
        void ReleaseMemory();
    }
}
