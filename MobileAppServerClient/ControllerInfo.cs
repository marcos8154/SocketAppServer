using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServerClient
{
    public class ControllerInfo
    {
        [JsonProperty]
        public string ControllerName { get; private set; }
        [JsonProperty]
        public string ControllerClass { get; private set; }
        [JsonProperty]
        public List<string> ControllerActions { get; private set; }

        public ControllerInfo()
        {
            ControllerActions = new List<string>();
        }
    }
}
