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
