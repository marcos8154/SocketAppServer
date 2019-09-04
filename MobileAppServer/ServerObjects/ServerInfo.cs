using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace MobileAppServer.ServerObjects
{
    internal class ServerInfo
    {
        public string MachineName { get; set; }
        public string ServerVersion { get; set; }
        public List<ControllerInfo> ServerControllers { get; set; }

        public ServerInfo()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            ServerVersion = version;
            ServerControllers = new List<ControllerInfo>();
            MachineName = Environment.MachineName;
        }
    }
}
