using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Events
{
    public class HardwareUsage
    {
        public double CPUUsage { get; private set; }

        public double MemoryUsageMegabytes { get; private set; }

        public double CurrentThreadCount { get; private set; }

        public DateTime CollectedTime { get; private set; }

        public HardwareUsage(double cpuUsage, double memoryUsage,
            double currentThreadCount)
        {
            CPUUsage = cpuUsage;
            MemoryUsageMegabytes = memoryUsage;
            CurrentThreadCount = currentThreadCount;
            CollectedTime = DateTime.Now;
        }
    }
}
