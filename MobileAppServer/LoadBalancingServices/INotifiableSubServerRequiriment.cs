using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.LoadBalancingServices
{
    public interface INotifiableSubServerRequirement
    {
        SubServer StartNewInstance();

        void StopInstance(SubServer server);
    }
}
