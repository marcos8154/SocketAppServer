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

using SocketAppServer.CoreServices.Logging;
using SocketAppServer.ManagedServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace SocketAppServer.CoreServices.CoreServer
{
    public class RequestPreProcessor : IDisposable
    {
        private ILoggingService logger = null;
        private ICoreServerService coreServer = null;
        private IEncodingConverterService encoder = null;

        RequestProcessor requestProcessor = null;
        BasicControllerRequestProcess basicControllerRequestProcessor = null;

        internal Socket clientSocket = null;
        private SocketSession session = null;
        private string uriRequest = null;

        private IAsyncResult asyncResult;

        public RequestPreProcessor(IAsyncResult AR)
        {
            asyncResult = AR;
        }

        private bool Initialize()
        {
            IServiceManager serviceManager = ServiceManager.GetInstance();
            logger = serviceManager.GetService<ILoggingService>();
            coreServer = serviceManager.GetService<ICoreServerService>("realserver");
            encoder = serviceManager.GetService<IEncodingConverterService>();

            clientSocket = (Socket)asyncResult.AsyncState;
            int received;

            TryGetSession();
            if (session == null)
            {
                Dispose();
                return false;
            }

            received = Receive();
            if (received == 0)
                return false;

            byte[] receivedBuffer = new byte[received];
            Array.Copy(session.SessionStorage, receivedBuffer, received);
            uriRequest = encoder.ConvertToString(receivedBuffer);

            return true;
        }

        private void TryGetSession()
        {
            for (int i = 0; i < 3; i++)
            {
                session = coreServer.GetSession(clientSocket);
                if (session != null)
                    return;
                else Thread.Sleep(100);
            }
        }

        private int Receive()
        {
            try
            {
                int received = clientSocket.EndReceive(asyncResult);
                return received;
            }
            catch (Exception ex)
            {
                logger.WriteLog($"[INTERNAL]: EndReceive async-reader client socket was thrown: {ex.Message}", ServerLogType.ERROR);
                Dispose();
                return 0;
            }
        }

        private void BasicControllerProcess()
        {
            basicControllerRequestProcessor = new BasicControllerRequestProcess(this);
            if (coreServer.GetConfiguration().IsSingleThreaded)
            {
                basicControllerRequestProcessor.DoInBackGround(uriRequest);
                Dispose();
            }
            else
            {
                basicControllerRequestProcessor.OnCompleted += BasicControllerRequestProc_OnCompleted;
                WaitPendingRequestsCompletations();
                basicControllerRequestProcessor.Execute(uriRequest);
            }
        }

        internal void Process()
        {
            bool initialized = Initialize();
            if (!initialized)
            {
                Dispose();
                return;
            }

            if (string.IsNullOrEmpty(uriRequest))
            {
                Dispose();
                return;
            }

            if (coreServer.IsBasicServerEnabled())
            {
                BasicControllerProcess();
                return;
            }

            requestProcessor = new RequestProcessor(ref uriRequest, ref clientSocket);

            if (coreServer.GetConfiguration().IsSingleThreaded)
            {
                requestProcessor.DoInBackGround(0);
                requestProcessor.OnPostExecute(0);
                Dispose();
            }
            else
            {
                requestProcessor.OnCompleted += RequestProcessor_OnCompleted;
                WaitPendingRequestsCompletations();
                requestProcessor.Execute(0);
            }
        }

        private void WaitPendingRequestsCompletations()
        {
            int maxThreadsCount = coreServer.GetConfiguration().MaxThreadsCount;
            while (maxThreadsCount > 0 &&
                RequestProcessor.ThreadCount >= maxThreadsCount)
            {
               if (!CSL.ConsoleDisabled) Console.ForegroundColor = ConsoleColor.Yellow;
                logger.WriteLog("\nThe number of current threads has exceeded the limit configured for this server. Waiting for pending requests completation...", ServerLogType.ALERT);
               if (!CSL.ConsoleDisabled) Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(300);
            }
        }

        private void BasicControllerRequestProc_OnCompleted(string result)
        {
            Dispose();
            basicControllerRequestProcessor.OnCompleted -= BasicControllerRequestProc_OnCompleted;
        }

        private void RequestProcessor_OnCompleted(object result)
        {
            Dispose();
            requestProcessor.OnCompleted -= RequestProcessor_OnCompleted;
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
                uriRequest = null;
                if (session != null)
                    coreServer.RemoveSession(ref session);
            }
            disposed = true;
        }
    }
}
