/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using SocketAppServer.CoreServices;
using SocketAppServer.LoadBalancingServices;
using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SocketAppServer.ServerObjects
{
    internal class ServerInfo
    {
        public string MachineName { get; set; }
        public string ServerVersion { get; set; }
        public bool RequiresAuthentication { get; set; }
        public bool IsLoadBanancingServer { get; set; }
        public List<ControllerInfo> ServerControllers { get; set; }

        public ServerInfo()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            ISecurityManagementService securityManagement = manager.GetService<ISecurityManagementService>();
            ICoreServerService coreServer = manager.GetService<ICoreServerService>("realserver");

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            if (coreServer.IsLoadBalanceEnabled())
                IsLoadBanancingServer = true;

            RequiresAuthentication = securityManagement.IsAuthenticationEnabled();
            ServerVersion = version;
            ServerControllers = new List<ControllerInfo>();
            MachineName = Environment.MachineName;
        }
    }
}
