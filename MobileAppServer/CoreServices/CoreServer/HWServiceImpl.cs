using MobileAppServer.ManagedServices;
using MobileAppServer.TelemetryServices;
using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.CoreServer
{
    internal class HWServiceImpl : IHardwareServices
    {
        ITelemetryServicesProvider telemetry = null;
        ICoreServerService coreServer = null;
        public HWServiceImpl()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            telemetry = manager.GetService<ITelemetryServicesProvider>();
            coreServer = manager.GetService<ICoreServerService>();
        }

        public double AverageCPUUsage(int lastHours = 3)
        {
            DateTime startDate = DateTime.Now.AddHours(-lastHours);
            DateTime endDate = DateTime.Now;
            IEnumerable<HardwareUsage> events = telemetry.GetHardwareUsages(startDate, endDate);
            return events.Average(evt => evt.CPUUsage);
        }

        public double AverageMemoryUsage(int lastHours = 3)
        {
            DateTime startDate = DateTime.Now.AddHours(-lastHours);
            DateTime endDate = DateTime.Now;
            IEnumerable<HardwareUsage> events = telemetry.GetHardwareUsages(startDate, endDate);
            return events.Average(evt => evt.MemoryUsageMegabytes);
        }

        public void ReleaseMemory()
        {
            int retrieves = 0;
            while(coreServer.CurrentThreadsCount() > 0 && retrieves < 3)
            {
                Thread.Sleep(1000);
                retrieves += 1;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
