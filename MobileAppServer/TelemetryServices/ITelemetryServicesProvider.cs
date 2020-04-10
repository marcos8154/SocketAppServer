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
