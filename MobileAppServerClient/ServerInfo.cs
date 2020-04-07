using System;
using System.Collections.Generic;

namespace MobileAppServerClient
{
    public class ServerInfo
    {
        public string MachineName { get; private set; }
        public string ServerVersion { get; private set; }
        public bool RequiresAuthentication { get; private set; }
        public bool IsLoadBanancingServer { get; private set; }

        public List<ControllerInfo> ServerControllers { get; private set; }

        public ServerInfo()
        {
            ServerControllers = new List<ControllerInfo>();
        }
    }
}
