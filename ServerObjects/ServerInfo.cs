using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    internal class ServerInfo
    {
        public string ServerVersion { get; set; }
        public List<ControllerInfo> ServerControllers { get; set; }

        public ServerInfo()
        {
            ServerVersion = "1.2.10";
            ServerControllers = new List<ControllerInfo>();
        }
    }
}
