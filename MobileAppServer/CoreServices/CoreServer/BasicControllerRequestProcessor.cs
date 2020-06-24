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
using SocketAppServer.ServerObjects;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace SocketAppServer.CoreServices.CoreServer
{
    /// <summary>
    /// Basic request processor, works dedicated with a single controller type and does not support advanced features like the standard processor (RequestProcessor)
    /// </summary>
    public class BasicControllerRequestProcess : AsyncTask<string, string, string>
    {
        private IServiceManager serviceManager = null;
        private ILoggingService logger = null;
        private ICoreServerService coreServer = null;
        private IBasicServerController basicController = null;
        private IEncodingConverterService encoder = null;

        public BasicControllerRequestProcess(RequestPreProcessor preProcessor)
        {
            serviceManager = ServiceManager.GetInstance();
            logger = serviceManager.GetService<ILoggingService>();
            basicController = serviceManager.GetService<IBasicServerController>();
            coreServer = serviceManager.GetService<ICoreServerService>("realserver");
            encoder = serviceManager.GetService<IEncodingConverterService>();

            this.preProcessor = preProcessor;
        }

        private RequestPreProcessor preProcessor = null;

        public override string DoInBackGround(string receivedData)
        {
            try
            {
                Stopwatch s = new Stopwatch();
                s.Start();
                ActionResult result = basicController.RunAction(receivedData);
                s.Stop();

                string resultToJson = JsonConvert.SerializeObject(result, AppServerConfigurator.SerializerSettings);

                byte[] resultBytes = coreServer.GetConfiguration().ServerEncoding.GetBytes(resultToJson);
                preProcessor.clientSocket.Send(resultBytes);
            }
            catch (Exception ex)
            {
                preProcessor.clientSocket.Send(encoder.ConvertToByteArray($"Error: {ex.Message}"));
                logger.WriteLog($"Basic Server Module Error: {ex.Message}", ServerLogType.ERROR);
            }

            preProcessor.Dispose();
            return string.Empty;
        }

        public override void OnPostExecute(string result)
        {
        }

        public override void OnPreExecute()
        {
        }

        public override void OnProgressUpdate(string progress)
        {
        }
    }
}
