using MobileAppServer.CoreServices;
using MobileAppServer.CoreServices.Logging;
using MobileAppServer.ManagedServices;
using MobileAppServer.ScheduledServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Impl
{
    public class TelemetryProcessorTask : ScheduledTask
    {
        private ITelemetryServicesProvider telemetry;
        private ITelemetryDataCollector collector;
        private ILoggingService logging;
        public TelemetryProcessorTask()
            : base("TelemetryProcessor", false, new ScheduledTaskInterval(0, 0, 3, 0), false)
        {
            IServiceManager manager = ServiceManager.GetInstance();
            telemetry = manager.GetService<ITelemetryServicesProvider>();
            collector = manager.GetService<ITelemetryDataCollector>();
            logging = manager.GetService<ILoggingService>();
        }

        public override void RunTask()
        {
            try
            {
                telemetry.ActionExecutionTime(collector.GetActionExecutions());
                telemetry.ActionError(collector.GetActionErros());
                telemetry.HWUsage(collector.GetHardwareUsages());
                telemetry.DependencyInjectionExecutiontime(collector.GetDependencyInjectors());
                telemetry.InterceptorExecutionTime(collector.GetInterceptorExecutions());
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += $"\n{ex.InnerException.Message}";
                logging.WriteLog($"TelemetryProcessorTask error: {msg}", ServerLogType.ERROR);
            }
        }
    }
}
