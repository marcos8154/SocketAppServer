using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Events
{
    public sealed class ActionExecutionTime
    {
        public string ControllerName { get; private set; }
        public string ActionName { get; private set; }
        public string ElapsedTime { get; private set; }
        public DateTime CollectTime { get; private set; }

        public ActionExecutionTime(string controllerName,
            string actionName,
            long elapsedTime)
        {
            ControllerName = controllerName;
            ActionName = actionName;
            ElapsedTime = $"{ elapsedTime} ms";
            CollectTime = DateTime.Now;
        }
    }
}
