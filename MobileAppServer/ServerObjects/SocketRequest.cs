
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace MobileAppServer.ServerObjects
{
    internal class SocketRequest
    {
        public Socket Client { get; set; }
        public List<RequestParameter> Parameters { get; set; }
        public IController Controller { get; set; }
        public string Action { get; set; }
        public bool HasErrors { get; set; }
        public string InternalErrorMessage { get; set; }

        public void ProcessResponse(ActionResult result, Socket socket)
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
