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

using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.ManagedServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketAppServer.ServerUtils;

namespace SocketAppServer.ServerObjects
{
    public class SocketRequest
    {
        internal Socket ClientSocket { get; set; }
        public IPEndPoint LocalEndPoint
        {
            get
            {
                if (ClientSocket == null)
                    return null;
                return (ClientSocket.LocalEndPoint as IPEndPoint);
            }

        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                if (ClientSocket == null)
                    return null;
                return (ClientSocket.RemoteEndPoint as IPEndPoint);
            }
        }


        private ILoggingService logger = null;
        private IEncodingConverterService encoder = null;
        private ICoreServerService coreServer = null;
        private IMemoryResponseStorage responseStorage = null;
        public SocketRequest()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            logger = manager.GetService<ILoggingService>();
            encoder = manager.GetService<IEncodingConverterService>();
            coreServer = manager.GetService<ICoreServerService>("realserver");
            responseStorage = manager.GetService<IMemoryResponseStorage>();
        }

        internal SocketRequest(IController controller,
            string action, List<RequestParameter> parameters, Socket clientSocket)
        {
            Controller = controller;
            Action = action;
            requestParameters = parameters;
            ClientSocket = clientSocket;
            InitializeServices();
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

        private string ResponseStorageId { get; set; }

        internal void ProcessResponse(ActionResult result, Socket socket,
            string sessionStorageId)
        {
            ResponseStorageId = sessionStorageId;
            ComputeStatisticks(result);
            ClientSocket = socket;
            ProcessResponse(result);
        }

        internal void ComputeStatisticks(ActionResult result)
        {
            if (AppServerConfigurator.DisableStatisticsCalculating)
                return;
            if (result.Type == 1)
                return;
            StringBuilder json = new StringBuilder();

            using (StringWriter sw = new StringWriter(json))
            {
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.ApplyCustomSettings();
                    js.Serialize(jw, result.Content);
                }
            }

            int bufferSize = coreServer.GetConfiguration().BufferSize;
            byte[] bytes = encoder.ConvertToByteArray(json.ToString());
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
            coreServer.RemoveSession(ref session);

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
            if (!string.IsNullOrEmpty(ResponseStorageId))
            {
                if (TypedObjectsRequestManager.IsSimpleType(response.Content.GetType()))
                    responseStorage.CreateStorage(ResponseStorageId, response.Content.ToString());
                else
                    responseStorage.CreateStorage(ResponseStorageId, JsonConvert.SerializeObject(response.Content, AppServerConfigurator.SerializerSettings));
                response.Content = null;
            }

            string json = JsonConvert.SerializeObject(response, AppServerConfigurator.SerializerSettings);
            byte[] resultData = encoder.ConvertToByteArray(json);
            SendBytes(resultData);
            json = null;
            resultData = null;
            response = null;
        }
    }
}
