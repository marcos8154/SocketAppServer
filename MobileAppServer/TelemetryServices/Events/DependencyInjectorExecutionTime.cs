using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Events
{
    public class DependencyInjectorExecutionTime
    {
        public string ControllerName { get; private set; }
        public string ElapsedMs { get; }
        public DateTime CalledTime { get; private set; }

        public DependencyInjectorExecutionTime(string controllerName,
            long elapsedMs)
        {
            ControllerName = controllerName;
            ElapsedMs = $"{elapsedMs} ms";
            CalledTime = DateTime.Now;
        }
    }
}
