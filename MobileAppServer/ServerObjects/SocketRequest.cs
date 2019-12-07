using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;

namespace MobileAppServer.ServerObjects
{
    public class SocketRequest
    {
        internal Socket Client { get; set; }

        public SocketRequest()
        {

        }

        internal SocketRequest(IController controller,
            string action, List<RequestParameter> parameters)
        {
            Controller = controller;
            Action = action;
            requestParameters = parameters;
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
            CheckStatistics(result);
            Client = socket;
            ProcessResponse(result);
        }

        internal void CheckStatistics(ActionResult result)
        {
            string json = JsonConvert.SerializeObject(result.Content);
            var bytes = Server.GlobalInstance.ServerEncoding.GetBytes(json);
            var lenght = bytes.Length;
            double percentBufferUsed = (lenght / (double)Server.GlobalInstance.BufferSize) * 100;
            result.ResponseLenght = lenght;
            result.PercentUsage = percentBufferUsed;

            if (percentBufferUsed >= 100)
            {
                string msg = $@"
The action response on the controller is using around {percentBufferUsed.ToString("N2")}% of the buffer quota configured on the server.
Server Buffer Size: {Server.GlobalInstance.BufferSize}
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
            var session = Server.GlobalInstance.ClientSockets.FirstOrDefault(c => c.ClientSocket.Equals(Client));
            Client.Send(data);

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
            var session = Server.GlobalInstance.ClientSockets.FirstOrDefault(c => c.ClientSocket.Equals(Client));
            if (session != null)
            {
                session.Close();
                Server.GlobalInstance.ClientSockets.Remove(session);
            }

            if (Client != null)
            {
                Client.Close();
                Client.Dispose();
                Client = null;
            }
        }

        private void ProcessResponse(ActionResult response)
        {
            try
            {
                /*
                if (response.Type == 1)
                {
                    byte[] file = (byte[])response.Content;
              //      response.Content = response.Content;
                    response.FileState = (file == null
                        ? "EOF"
                        : "BOF");

                    string jsonFile = JsonConvert.SerializeObject(response);
                    SendBytes(Server.GlobalInstance.ServerEncoding.GetBytes(jsonFile));
                    if (file != null)
                        SendBytes(file);
                    return;
                }
                */
                string json = JsonConvert.SerializeObject(response);
                byte[] resultData = Server.GlobalInstance.ServerEncoding.GetBytes(json);
                SendBytes(resultData);
                json = null;
                resultData = null;
            }
            catch (Exception ex)
            {
                LogController.WriteLog(new ServerLog($"*** ERROR ON PROCESS RESPONSE:*** \n {ex.Message}", ServerLogType.ERROR));
                ReleaseSession();
            }
        }
    }
}
