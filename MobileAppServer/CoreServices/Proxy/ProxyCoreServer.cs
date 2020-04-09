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
