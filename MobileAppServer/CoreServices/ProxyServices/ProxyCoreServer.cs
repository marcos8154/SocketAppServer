using MobileAppServer.CoreServices.CoreServer;
using MobileAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.ProxyServices
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

        public void RemoveSession(SocketSession session)
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
    }
}
