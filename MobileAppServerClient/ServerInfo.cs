using System;
using System.Collections.Generic;

namespace MobileAppServerClient
{
    public class ServerInfo
    {
        public string MachineName { get; set; }
        public string ServerVersion { get; set; }
        public List<ControllerInfo> ServerControllers { get; set; }

        public ServerInfo()
        {
        }
    }
}
