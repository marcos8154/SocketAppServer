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

using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.ProxyServices
{
    internal class ProxyCoreServer : ICoreServerService
    {
        private string instanceStr;
        private ICoreServerService realServer;

        public override string ToString()
        {
            return instanceStr;
        }

        public ProxyCoreServer()
        {
            instanceStr = $"ProxyCoreServer_{Guid.NewGuid().ToString().Replace("-", "")}";
            realServer = ServiceManager.GetInstance().GetService<ICoreServerService>("realserver");
        }

        public void AcceptCallback(IAsyncResult AR)
        {
            throw new InvalidOperationException("External modules cannot make this call in the server's kernel");
        }

        public int CurrentThreadsCount()
        {
            return realServer.CurrentThreadsCount();
        }

        public void EnableBasicServerProcessorMode(Type basicProccessorType)
        {
            throw new InvalidOperationException("External modules cannot make this call in the server's kernel");
        }

        public ServerConfiguration GetConfiguration()
        {
            return realServer.GetConfiguration();
        }

        public IReadOnlyCollection<SocketSession> GetCurrentSessions()
        {
            return realServer.GetCurrentSessions();
        }

        public SocketSession GetSession(Socket clientSocket)
        {
            return realServer.GetSession(clientSocket);
        }

        public bool IsBasicServerEnabled()
        {
            return realServer.IsBasicServerEnabled();
        }

        public bool IsLoadBalanceEnabled()
        {
            return realServer.IsLoadBalanceEnabled();
        }

        public bool IsServerStarted()
        {
            return realServer.IsServerStarted();
        }

        public void Reboot()
        {
            throw new InvalidOperationException("External modules cannot make this call in the server's kernel");
        }

        public void ReceiveCallback(IAsyncResult AR)
        {
            throw new InvalidOperationException("External modules cannot make this call in the server's kernel");
        }

        public void RemoveSession(ref SocketSession session)
        {
            throw new InvalidOperationException("External modules cannot make this call in the server's kernel");
        }

        public void RunServerStartupTasks()
        {
            throw new InvalidOperationException("External modules cannot make this call in the server's kernel");
        }

        public void SetConfiguration(ServerConfiguration configuration)
        {
            realServer.SetConfiguration(configuration);
        }

        public void Start()
        {
            throw new InvalidOperationException("External modules cannot make this call in the server's kernel");
        }

        public string GetServerVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void DisableTelemetryServices()
        {
            realServer.DisableTelemetryServices();
        }

        public void Stop()
        {
            realServer.Stop();
        }
    }
}
