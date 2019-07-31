using System;
using System.Collections.Generic;

namespace MobileAppServer.ServerObjects
{
    internal class ServerInfo
    {
        public string MachineName { get; set; }
        public string ServerVersion { get; set; }
        public List<ControllerInfo> ServerControllers { get; set; }

        public ServerInfo()
        {
            ServerVersion = "1.2.10";
            ServerControllers = new List<ControllerInfo>();
            MachineName = Environment.MachineName;
        }
    }
}
