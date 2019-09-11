using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public class RequestBody
    {
        [JsonProperty]
        public string Action { get; internal set; }

        [JsonProperty]
        public string Controller { get; internal set; }

        [JsonProperty]
        public List<RequestParameter> Parameters { get; internal set; }
    }
}
