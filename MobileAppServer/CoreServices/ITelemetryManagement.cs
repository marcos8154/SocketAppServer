using MobileAppServer.TelemetryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    public interface ITelemetryManagement
    {
        void Initialize();

        void SetProviderType(Type providerType);
    }
}
