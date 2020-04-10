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
