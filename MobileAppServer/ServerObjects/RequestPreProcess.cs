using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace MobileAppServer.ServerObjects
{
    public class RequestPreProcess : IDisposable
    {
        private SocketSession GetSession(Socket clientSocket)
        {
            try
            {
                var session = Server.GlobalInstance.ClientSockets.FirstOrDefault(c => c.ClientSocket.Equals(clientSocket));
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
                {
                    try
                    {
                        Server.GlobalInstance.ClientSockets.Remove(session);
                        session.Close();
                        session = null;
                    }
                    catch { }
                }

                if (ClientSocket != null)
                {
                    try
                    {
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
            while (Server.GlobalInstance.MaxThreadsCount > 0 &&
                RequestProccess.ThreadCount >= Server.GlobalInstance.MaxThreadsCount)
            {
                LogController.WriteLog(new ServerLog("\nNumber of current threads has exceeded the set limit. Waiting for tasks to finish...", ServerLogType.ALERT));
                Thread.Sleep(300);
            }
        }

        internal Socket ClientSocket { get; private set; }
        SocketSession session = null;

        public RequestPreProcess(IAsyncResult AR)
        {
            ClientSocket = (Socket)AR.AsyncState;
            int received;

            for (int i = 0; i < 3; i++)
            {
                session = GetSession(ClientSocket);
                if (session != null)
                    break;
                else Thread.Sleep(100);
            }

            if (session == null)
            {
                Dispose();
                return;
            }

            try
            {
                received = ClientSocket.EndReceive(AR);
            }
            catch (Exception ex)
            {
                Dispose();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(session.SessionStorage, recBuf, received);
            string uriRequest = Server.GlobalInstance.ServerEncoding.GetString(recBuf);
            string resultText = string.Empty;

            var srv = Server.GlobalInstance;
            if (srv.IsBasicServerControllerMode)
            {
                BasicControllerRequestProcess requestProcess = new BasicControllerRequestProcess(this);
                if (srv.IsSingleThreaded)
                    requestProcess.DoInBackGround(uriRequest);
                else
                {
                    WaitPendingRequestsCompletations();
                    requestProcess.Execute(uriRequest);
                }
                return;
            }

            if (string.IsNullOrEmpty(uriRequest))
            {
                Dispose();
                return;
            }

            RequestProccess process = new RequestProccess(uriRequest, ClientSocket);
            process.OnCompleted += Process_OnCompleted;

            if (Server.GlobalInstance.IsSingleThreaded)
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

        private void Process_OnCompleted(object result)
        {
            Dispose();
        }
    }
}
