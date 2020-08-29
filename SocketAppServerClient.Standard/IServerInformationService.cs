using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    public interface IServerInformationService
    {
        /// <summary>
        /// Obtains the average (in %) of the CPU usage on the server in the last X minutes. 
        /// Note that this only works if telemetry services on the server are enabled
        /// </summary>
        /// <param name="lastMinutes"></param>
        /// <returns></returns>
        double ServerAverageCPUUsage(int lastMinutes = 3);

        /// <summary>
        /// Obtains the average (in %) of the memory usage on the server in the last X minutes. 
        /// Note that this only works if telemetry services on the server are enabled
        /// </summary>
        /// <param name="lastMinutes"></param>
        /// <returns></returns>
        double ServerAverageMemoryUsage(int lastMinutes = 3);

        /// <summary>
        /// Obtains the number of failed requests on the server for the specified controller and action
        /// Note that this only works if telemetry services on the server are enabled
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        int RequestsErrorsCount(string controllerName, string actionName);

        /// <summary>
        /// Obtains the number of succeded requests on the server for the specified controller and action
        /// Note that this only works if telemetry services on the server are enabled
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        int RequestsSuccessCount(string controllerName, string actionName);


        /// <summary>
        /// Get the number of active threads on the server
        /// </summary>
        /// <returns></returns>
        long GetCurrentThreadsCount();

        /// <summary>
        /// Get the parameters of an action inside a controller on the server
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        string[] GetActionParameters(string controller, string action);

        /// <summary>
        /// Get general information about the server
        /// </summary>
        /// <returns></returns>
        ServerInfo GetFullServerInfo();
    }
}
