using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace MobileAppServer.ServerObjects
{
    public class BasicControllerRequestProcess : AsyncTask<string, string, string>
    {
        public BasicControllerRequestProcess(RequestPreProcess preProcess)
        {
            PreProcess = preProcess;
        }

        public RequestPreProcess PreProcess { get; }

        public override string DoInBackGround(string receivedData)
        {
            var srv = Server.GlobalInstance;
            LogController.WriteLog(new ServerLog($"Basic Server Module: Begin Request"));

            IBasicServerController controller = (IBasicServerController)Activator.CreateInstance(srv.BasicServerControllerType, null);
            try
            {
                Stopwatch s = new Stopwatch();
                s.Start();
                ActionResult result = controller.RunAction(receivedData);
                s.Stop();

                string resultToJson = JsonConvert.SerializeObject(result);

                byte[] resultBytes = srv.ServerEncoding.GetBytes(resultToJson);
                PreProcess.ClientSocket.Send(resultBytes);
                LogController.WriteLog(new ServerLog($"Request completed in ~{s.ElapsedMilliseconds}ms"));
            }
            catch (Exception ex)
            {
                PreProcess.ClientSocket.Send(srv.ServerEncoding.GetBytes($"Error: {ex.Message}"));
                LogController.WriteLog(new ServerLog($"Basic Server Module Error: {ex.Message}", ServerLogType.ERROR));
            }

            PreProcess.Dispose();

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
