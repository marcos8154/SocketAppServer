using MobileAppServer.CoreServices;
using MobileAppServer.CoreServices.Logging;
using MobileAppServer.ManagedServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MobileAppServer.ServerObjects
{
    public class SocketRequest
    {
        internal Socket ClientSocket { get; set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }


        private ILoggingService logger = null;
        private IEncodingConverterService encoder = null;
        private ICoreServerService coreServer = null;
        public SocketRequest()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            logger = manager.GetService<ILoggingService>();
            encoder = manager.GetService<IEncodingConverterService>();
            coreServer = manager.GetService<ICoreServerService>("realserver");
        }

        internal SocketRequest(IController controller,
            string action, List<RequestParameter> parameters, Socket clientSocket)
        {
            Controller = controller;
            Action = action;
            requestParameters = parameters;
            ClientSocket = clientSocket;

            LocalEndPoint = clientSocket.LocalEndPoint as IPEndPoint;
            RemoteEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
        }

        private List<RequestParameter> requestParameters = new List<RequestParameter>();
        internal void AddParameter(RequestParameter parameter)
        {
            requestParameters.Add(parameter);
        }

        public RequestParameter FindParameter(string parameterName)
        {
            var parameter = requestParameters.FirstOrDefault(p => p.Name.Equals(parameterName));
            return parameter;
        }

        public ReadOnlyCollection<RequestParameter> Parameters
        {
            get
            {
                if (requestParameters == null)
                    requestParameters = new List<RequestParameter>();
                return new ReadOnlyCollection<RequestParameter>(requestParameters);
            }
        }
        public IController Controller { get; internal set; }
        public string Action { get; internal set; }
        public bool HasErrors { get; internal set; }
        public string InternalErrorMessage { get; internal set; }

        internal void ProcessResponse(ActionResult result, Socket socket)
        {
            ComputeStatisticks(result);
            ClientSocket = socket;
            ProcessResponse(result);
        }

        internal void ComputeStatisticks(ActionResult result)
        {
            if (result.Type == 1)
                return;
            string json = JsonConvert.SerializeObject(result.Content);

            int bufferSize = coreServer.GetConfiguration().BufferSize;
            byte[] bytes = encoder.ConvertToByteArray(json);
            int lenght = bytes.Length;
            double percentBufferUsed = (lenght / (double)bufferSize) * 100;
            result.ResponseLenght = lenght;
            result.PercentUsage = percentBufferUsed;

            if (percentBufferUsed >= 100)
            {
                string msg = $@"
The action response on the controller is using around {percentBufferUsed.ToString("N2")}% of the buffer quota configured on the server.
Server Buffer Size: {bufferSize}
Response Size:      {lenght}
Operation has stopped.";

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Gray;

                var alert = new ServerAlert
                {
                    Action = Action,
                    Controller = Controller.GetType().Name,
                    Date = DateTime.Now,
                    Message = msg
                };

                ServerAlertManager.CreateAlert(alert);
                throw new Exception(msg);
            }

            if (percentBufferUsed >= 80)
            {
                string msg = $"\nThe action response on the controller is using around {percentBufferUsed.ToString("N2")}% of the buffer quota configured on the server. Review your code as soon as possible before the server collapses and begins to give incomplete responses to connected clients.";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Gray;

                var alert = new ServerAlert
                {
                    Action = Action,
                    Controller = Controller.GetType().Name,
                    Date = DateTime.Now,
                    Message = msg
                };

                ServerAlertManager.CreateAlert(alert);
            }
        }

        private void SendBytes(byte[] data)
        {
            var session = coreServer.GetSession(ClientSocket);
            ClientSocket.Send(data);

            ReleaseSession();
            /*
            Client.BeginReceive(session.SessionStorage, 0,
                Server.GlobalInstance.BufferSize,
                SocketFlags.None,
                Server.GlobalInstance.ReceiveCallback,
                Client); */
        }

        private void ReleaseSession()
        {
            var session = coreServer.GetSession(ClientSocket);
            coreServer.RemoveSession(session);

            if (ClientSocket != null)
            {
                ClientSocket.Close();
                ClientSocket.Dispose();
                ClientSocket = null;
            }
        }

        private void ProcessResponse(ActionResult response)
        {
            try
            {
                if (response.Type == 0)
                    SendJsonResponse(response);
                if (response.Type == 1)
                    SendFileResponse(response);
            }
            catch (Exception ex)
            {
                logger.WriteLog($"*** ERROR ON PROCESS RESPONSE:*** \n {ex.Message}", ServerLogType.ERROR);
                ReleaseSession();
            }
        }

        private void SendFileResponse(ActionResult response)
        {
            byte[] resultData = (byte[])response.Content;
            SendBytes(resultData);
            resultData = null;
            response = null;
        }

        private void SendJsonResponse(ActionResult response)
        {
            string json = JsonConvert.SerializeObject(response);
            byte[] resultData = encoder.ConvertToByteArray(json);
            SendBytes(resultData);
            json = null;
            resultData = null;
            response = null;
        }
    }
}
