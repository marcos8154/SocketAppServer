using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Events
{
    public sealed class ActionExecution
    {
        public string ControllerName { get; private set; }
        public string ActionName { get; private set; }
        public int ElapsedTime { get; private set; }
        public DateTime CalledDateTime { get; private set; }
        public IReadOnlyCollection<RequestParameter> Parameters { get; private set; }

        public ActionExecution(string controllerName, 
            string actionName, 
            int elapsedTime, 
            DateTime calledDateTime, 
            List<RequestParameter> parameters)
        {
            ControllerName = controllerName;
            ActionName = actionName;
            ElapsedTime = elapsedTime;
            CalledDateTime = calledDateTime;
            Parameters = parameters.AsReadOnly();
        }
    }
}
