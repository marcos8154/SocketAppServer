
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

        private List<RequestParameter> requestParameters = new List<RequestParameter>();
        internal void AddParameter(RequestParameter parameter)
        {
            requestParameters.Add(parameter);
        }

        public ReadOnlyCollection<RequestParameter> Parameters
        {
            get
            {
                return new ReadOnlyCollection<RequestParameter>(requestParameters);
            }
        }
        public IController Controller { get; internal set; }
        public string Action { get; internal set; }
        public bool HasErrors { get; internal set; }
        public string InternalErrorMessage { get; internal set; }

        internal void ProcessResponse(ActionResult result, Socket socket)
        {
            Client = socket;
            ProcessResponse(result);
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
        }

        private void ProcessResponse(ActionResult response)
        {
            try
            {
                if (response.Type == 1)
                {
                    byte[] file = (byte[])response.Content;
                    response.Content = null;
                    response.FileState = (file == null
                        ? "EOF"
                        : "BOF");

                    SendBytes(Server.GlobalInstance.ServerEncoding.GetBytes(JsonConvert.SerializeObject(response)));
                    if (file != null)
                        SendBytes(file);
                    return;
                }

                byte[] resultData = Server.GlobalInstance.ServerEncoding.GetBytes(JsonConvert.SerializeObject(response));
                SendBytes(resultData);
            }
            catch (Exception ex)
            {
                LogController.WriteLog($"*** ERROR ON PROCESS RESPONSE:*** \n {ex.Message}");
                ReleaseSession();
            }
        }
    }
}
