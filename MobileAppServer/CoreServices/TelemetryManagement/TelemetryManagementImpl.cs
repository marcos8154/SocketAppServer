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

using SocketAppServer.ManagedServices;
using SocketAppServer.TelemetryServices;
using SocketAppServer.TelemetryServices.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.TelemetryManagement
{
    public class TelemetryManagementImpl : ITelemetryManagement
    {
        private Type provider;

        public TelemetryManagementImpl()
        {
            SetProviderType(typeof(DefaultTelemetryServicesProvider));
        }

        public void Initialize()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            manager.Bind<ITelemetryServicesProvider>(provider, true);
            manager.Bind<ITelemetryDataCollector>(typeof(TelemetryDataCollectorImpl), true);

            IScheduledTaskManager taskManager = manager.GetService<IScheduledTaskManager>();
            taskManager.AddScheduledTask(new HWUsageCollectorTask());
            taskManager.AddScheduledTask(new TelemetryProcessorTask());
        }

        public void SetProviderType(Type providerType)
        {
            provider = providerType;
        }
    }
}
