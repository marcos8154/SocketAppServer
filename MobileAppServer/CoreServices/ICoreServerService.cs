using MobileAppServer.CoreServices.CoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    internal interface ICoreServerService
    {
        void RunServerStartupTasks();
        void SetConfiguration(ServerConfiguration configuration);
        ServerConfiguration GetConfiguration();
        bool IsServerStarted();
        IReadOnlyCollection<SocketSession> GetCurrentSessions();
        void AcceptCallback(IAsyncResult AR);
        void ReceiveCallback(IAsyncResult AR);
        void Reboot();
        int CurrentThreadsCount();
        void EnableBasicServerProcessorMode(Type basicProccessorType);
        bool IsBasicServerEnabled();
        void Start();
        SocketSession GetSession(Socket clientSocket);
        void RemoveSession(SocketSession session);
        bool IsLoadBalanceEnabled();
    }
}
