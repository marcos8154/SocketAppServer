using MobileAppServer.ManagedServices;
using MobileAppServer.TelemetryServices;
using MobileAppServer.TelemetryServices.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.TelemetryManagement
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
