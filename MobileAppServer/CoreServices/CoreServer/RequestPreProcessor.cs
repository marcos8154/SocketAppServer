using MobileAppServer.CoreServices.Logging;
using MobileAppServer.ManagedServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace MobileAppServer.CoreServices.CoreServer
{
    public class RequestPreProcessor : IDisposable
    {
        private ILoggingService logger = null;
        private ICoreServerService coreServer = null;
        private IEncodingConverterService encoder = null;

        internal Socket ClientSocket { get; private set; }
        private SocketSession session = null;
        private string uriRequest = null;

        public RequestPreProcessor(IAsyncResult AR)
        {
            IServiceManager serviceManager = ServiceManagerFactory.GetInstance();
            logger = serviceManager.GetService<ILoggingService>();
            coreServer = serviceManager.GetService<ICoreServerService>();
            encoder = serviceManager.GetService<IEncodingConverterService>();

            ClientSocket = (Socket)AR.AsyncState;
            int received;

            session = TryGetSession();
            if (session == null)
            {
                Dispose();
                return;
            }

            received = Receive(AR);
            if (received == 0)
                return;

            byte[] receivedBuffer = new byte[received];
            Array.Copy(session.SessionStorage, receivedBuffer, received);
            uriRequest = encoder.ConvertToString(receivedBuffer);

            if (coreServer.IsBasicServerEnabled())
            {
                BasicControllerProcess();
                return;
            }

            if (string.IsNullOrEmpty(uriRequest))
            {
                Dispose();
                return;
            }

            Process();
        }

        private SocketSession TryGetSession()
        {
            SocketSession s = null;
            for (int i = 0; i < 3; i++)
            {
                s = GetSession(ClientSocket);
                if (s != null)
                    return s;
                else Thread.Sleep(100);
            }
            return null;
        }

        private int Receive(IAsyncResult AR)
        {
            try
            {
                int received = ClientSocket.EndReceive(AR);
                return received;
            }
            catch (Exception ex)
            {
                Dispose();
                return 0;
            }
        }

        private void BasicControllerProcess()
        {
            BasicControllerRequestProcess requestProcess = new BasicControllerRequestProcess(this);
            if (coreServer.GetConfiguration().IsSingleThreaded)
                requestProcess.DoInBackGround(uriRequest);
            else
            {
                WaitPendingRequestsCompletations();
                requestProcess.Execute(uriRequest);
            }
        }

        private void Process()
        {
            RequestProcessor process = new RequestProcessor(uriRequest, ClientSocket);
            process.OnCompleted += Process_OnCompleted;

            if (coreServer.GetConfiguration().IsSingleThreaded)
            {
                process.DoInBackGround(0);
                Dispose();
            }
            else
            {
                WaitPendingRequestsCompletations();
                process.Execute(0);
            }
        }

        private SocketSession GetSession(Socket clientSocket)
        {
            try
            {
                SocketSession session = coreServer.GetSession(clientSocket);
                return session;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                if (session != null)
                    coreServer.RemoveSession(session);

                if (ClientSocket != null)
                {
                    try
                    {
                        ClientSocket.Close();
                        ClientSocket.Dispose();
                        ClientSocket = null;
                    }
                    catch { }
                }
            }
            disposed = true;
        }

        private void WaitPendingRequestsCompletations()
        {
            int maxThreadsCount = coreServer.GetConfiguration().MaxThreadsCount;
            while (maxThreadsCount > 0 &&
                RequestProcessor.ThreadCount >= maxThreadsCount)
            {
                logger.WriteLog("\nNumber of current threads has exceeded the set limit. Waiting for tasks to finish...", ServerLogType.ALERT);
                Thread.Sleep(300);
            }
        }

        private void Process_OnCompleted(object result)
        {
            Dispose();
        }
    }
}
