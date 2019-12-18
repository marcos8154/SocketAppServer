using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                if (clientSocket != null)
                {
                    try
                    {
                        clientSocket.Dispose();
                        clientSocket = null;
                    }
                    catch { }
                }
            }
            disposed = true;
        }

        Socket clientSocket = null;
        SocketSession session = null;

        public RequestPreProcess(IAsyncResult AR)
        {
            clientSocket = (Socket)AR.AsyncState;
            int received;

            for (int i = 0; i < 3; i++)
            {
                session = GetSession(clientSocket);
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
                received = clientSocket.EndReceive(AR);
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

            if (string.IsNullOrEmpty(uriRequest))
            {
                Dispose();
                return;
            }

            RequestProccess process = new RequestProccess(uriRequest, clientSocket);
            process.OnCompleted += Process_OnCompleted;

            if (Server.GlobalInstance.IsSingleThreaded)
            {
                process.DoInBackGround(0);
                Dispose();
            }
            else
            {
                while (Server.GlobalInstance.MaxThreadsCount > 0 &&
                   RequestProccess.ThreadCount >= Server.GlobalInstance.MaxThreadsCount)
                {
                    LogController.WriteLog(new ServerLog("\nNumber of current threads has exceeded the set limit. Waiting for tasks to finish...", ServerLogType.ALERT));
                    Thread.Sleep(300);
                }

                process.Execute(0);
            }
        }

        private void Process_OnCompleted(object result)
        {
            Dispose();
        }
    }
}
