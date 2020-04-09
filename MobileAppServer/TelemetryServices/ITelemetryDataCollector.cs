using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices
{
    internal interface ITelemetryDataCollector
    {
        void Collect(object eventObj);

        List<ActionError> GetActionErros();
        List<ActionExecutionTime> GetActionExecutions();
        List<DependencyInjectorExecutionTime> GetDependencyInjectors();
        List<HardwareUsage> GetHardwareUsages();
        List<InterceptorExecutionTime> GetInterceptorExecutions();
    }
}
