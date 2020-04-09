using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices
{
    /// <summary>
    /// The telemetry service provider allows the storage, search and maintenance of the events collected
    /// </summary>
    public interface ITelemetryServicesProvider
    {
        /// <summary>
        /// Store/Persist collected ActionExecutionTime events
        /// </summary>
        /// <param name="actions"></param>
        void ActionExecutionTime(IEnumerable<ActionExecutionTime> actions);

        /// <summary>
        /// Store/Persist collected ActionError events
        /// </summary>
        /// <param name="error"></param>
        void ActionError(IEnumerable<ActionError> error);

        /// <summary>
        /// Store/Persist collected InterceptorExecutionTime events
        /// </summary>
        /// <param name="interceptors"></param>
        void InterceptorExecutionTime(IEnumerable<InterceptorExecutionTime> interceptors);

        /// <summary>
        /// Store/Persist collected DependencyInjectorExecutionTime events
        /// </summary>
        /// <param name="dependencyInjectors"></param>
        void DependencyInjectionExecutiontime(IEnumerable<DependencyInjectorExecutionTime> dependencyInjectors);

        /// <summary>
        /// Store/Persist collected HardwareUsage events
        /// </summary>
        /// <param name="usage"></param>
        void HWUsage(IEnumerable<HardwareUsage> usage);

        /// <summary>
        /// Retrieves a set of ActionError events persisted in a repository managed by the provider's implementation
        /// </summary>
        /// <returns></returns>
        IEnumerable<ActionError> GetActionErros(DateTime startDate, DateTime endDate, string controllerName, string actionName);

        /// <summary>
        /// Retrieves a set of ActionExecutionTime events persisted in a repository managed by the provider's implementation
        /// </summary>
        /// <returns></returns>
        IEnumerable<ActionExecutionTime> GetActionExecutionTimes(DateTime startDate , DateTime endDate , string controllerName , string actionName );

        /// <summary>
        /// Retrieves a set of DependencyInjectorExecutionTime events persisted in a repository managed by the provider's implementation
        /// </summary>
        /// <returns></returns>
        IEnumerable<DependencyInjectorExecutionTime> GetDependencyInjectorExecutionTimes(DateTime startDate , DateTime endDate , string controllerName );

        /// <summary>
        /// Retrieves a set of HardwareUsage events persisted in a repository managed by the provider's implementation
        /// </summary>
        /// <returns></returns>
        IEnumerable<HardwareUsage> GetHardwareUsages(DateTime startDate , DateTime endDate );

        /// <summary>
        /// Retrieves a set of HardwareUsage events persisted in a repository managed by the provider's implementation
        /// </summary>
        /// <returns></returns>
        IEnumerable<InterceptorExecutionTime> GetInterceptorExecutionTimes(DateTime startDate , DateTime endDate , string controllerName , string actionName );
    }
}
