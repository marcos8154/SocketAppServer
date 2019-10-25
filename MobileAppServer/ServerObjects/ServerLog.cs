using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public sealed class ServerLog
    {
        [JsonProperty]
        public DateTime EventDate { get; internal set; }

        [JsonProperty]
        public string LogText { get; internal set; }

        [JsonProperty]
        public string ControllerName { get; internal set; }

        [JsonProperty]
        public string ActionName { get; internal set; }

        [JsonProperty]
        public ServerLogType Type { get; internal set; }

        internal ServerLog(string logText, ServerLogType type = ServerLogType.INFO)
        {
            EventDate = DateTime.Now;
            LogText = logText;
            Type = type;
        }

        internal ServerLog(string logText, string controller, string action, ServerLogType type = ServerLogType.INFO)
        {
            LogText = logText;
            ControllerName = controller;
            ActionName = action;
            Type = type;
        }

        public ServerLog()
        {

        }
    }
}
