using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SocketAppServerClient
{
    public class ServerInfo
    {
        [JsonProperty]
        public string MachineName { get; private set; }

        [JsonProperty]
        public string ServerVersion { get; private set; }

        [JsonProperty]
        public bool RequiresAuthentication { get; private set; }

        [JsonProperty]
        public bool IsLoadBanancingServer { get; private set; }

        [JsonProperty]
        public List<ControllerInfo> ServerControllers { get; private set; }

        public ServerInfo()
        {
            ServerControllers = new List<ControllerInfo>();
        }
    }
}
