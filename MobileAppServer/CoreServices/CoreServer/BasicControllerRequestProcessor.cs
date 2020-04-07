using MobileAppServer.CoreServices.Logging;
using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace MobileAppServer.CoreServices.CoreServer
{
    public class BasicControllerRequestProcess : AsyncTask<string, string, string>
    {
        private IServiceManager serviceManager = null;
        private ILoggingService logger = null;
        private ICoreServerService coreServer = null;
        private IBasicServerController basicController = null;
        private IEncodingConverterService encoder = null;

        public BasicControllerRequestProcess(RequestPreProcessor preProcessor)
        {
            serviceManager = ServiceManagerFactory.GetInstance();
            logger = serviceManager.GetService<ILoggingService>();
            basicController = serviceManager.GetService<IBasicServerController>();
            coreServer = serviceManager.GetService<ICoreServerService>();
            encoder = serviceManager.GetService<IEncodingConverterService>();

            this.preProcessor = preProcessor;
        }

        private RequestPreProcessor preProcessor = null;

        public override string DoInBackGround(string receivedData)
        {
            logger.WriteLog($"Basic Server Module: Begin Request");

            try
            {
                Stopwatch s = new Stopwatch();
                s.Start();
                ActionResult result = basicController.RunAction(receivedData);
                s.Stop();

                string resultToJson = JsonConvert.SerializeObject(result);

                byte[] resultBytes = coreServer.GetConfiguration().ServerEncoding.GetBytes(resultToJson);
                preProcessor.ClientSocket.Send(resultBytes);
            }
            catch (Exception ex)
            {
                preProcessor.ClientSocket.Send(encoder.ConvertToByteArray($"Error: {ex.Message}"));
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
