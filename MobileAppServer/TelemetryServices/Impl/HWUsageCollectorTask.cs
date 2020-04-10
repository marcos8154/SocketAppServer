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
using MobileAppServer.ManagedServices;
using MobileAppServer.ScheduledServices;
using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Impl
{
    internal class HWUsageCollectorTask : ScheduledTask
    {
        private ITelemetryDataCollector dataCollector;
        private ICoreServerService coreServer;
        public HWUsageCollectorTask()
            : base("HWUsageCollector", true, new ScheduledTaskInterval(0, 0, 0, 1), false)
        {
            IServiceManager manager = ServiceManager.GetInstance();
            dataCollector = manager.GetService<ITelemetryDataCollector>();
            coreServer = manager.GetService<ICoreServerService>();
        }

        public override void RunTask()
        {
            double cpu = GetCPU();
            double memory = GetMemory();

            dataCollector.Collect(new HardwareUsage(cpu, memory, coreServer.CurrentThreadsCount()));
        }

        private double GetMemory()
        {
            double memory = GC.GetTotalMemory(false);
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = memory;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            //   return $"{len.ToString("N3")}{sizes[order]}";
            return double.Parse(len.ToString("N3"));
        }

        private double GetCPU()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            Thread.Sleep(500);
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return double.Parse((cpuUsageTotal * 100).ToString("N2"));

            //return $"{(cpuUsageTotal * 100).ToString("N2")}%";
        }
    }
}
