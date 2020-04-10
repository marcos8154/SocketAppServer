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

using MobileAppServer.CoreServices.CoreServer;
using MobileAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.Proxy
{
    public class ProxyCoreServer : ICoreServerService
    {
        public override string ToString()
        {
            return instanceNameStr;
        }

        private string instanceNameStr = null;
        private ICoreServerService realCoreServer = null;
        public ProxyCoreServer()
        {
            realCoreServer = ServiceManagerFactory.GetInstance().GetService<ICoreServerService>();
            instanceNameStr = $"ProxyCoreServer_{Guid.NewGuid().ToString().Replace("-", "")}";
        }

        public void AcceptCallback(IAsyncResult AR)
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public int CurrentThreadsCount()
        {
            return realCoreServer.CurrentThreadsCount();
        }

        public void EnableBasicServerProcessorMode(Type basicProccessorType)
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public ServerConfiguration GetConfiguration()
        {
            return realCoreServer.GetConfiguration();
        }

        public IReadOnlyCollection<SocketSession> GetCurrentSessions()
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public SocketSession GetSession(Socket clientSocket)
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public bool IsBasicServerEnabled()
        {
            return realCoreServer.IsBasicServerEnabled();
        }

        public bool IsLoadBalanceEnabled()
        {
            return realCoreServer.IsLoadBalanceEnabled();
        }

        public bool IsServerStarted()
        {
            return realCoreServer.IsServerStarted();
        }

        public void Reboot()
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public void ReceiveCallback(IAsyncResult AR)
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public void RemoveSession(SocketSession session)
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public void RunServerStartupTasks()
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }

        public void SetConfiguration(ServerConfiguration configuration)
        {
            realCoreServer.SetConfiguration(configuration);
        }

        public void Start()
        {
            throw new NotImplementedException("External modules cannot make this type of call in the server kernel");
        }
    }
}
