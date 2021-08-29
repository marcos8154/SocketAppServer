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
using System.Threading;

namespace SocketAppServer.ServerObjects
{
    public class SocketRequest
    {
        #region Properties
        public IController Controller { get; internal set; }
        public string Action { get; internal set; }
        public bool HasErrors { get; internal set; }
        public string InternalErrorMessage { get; internal set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        private ILoggingService logger = null;
        private IEncodingConverterService encoder = null;
        private ICoreServerService coreServer = null;
        private IControllerManager controllerManager = null;
        private IMemoryResponseStorage responseStorage = null;
        private List<RequestParameter> requestParameters = null;
        private string ResponseStorageId { get; set; }
        internal Socket ClientSocket { get; private set; }
        #endregion

        #region Constructors and helpers
        public SocketRequest()
        {
            requestParameters = new List<RequestParameter>();
            InitializeServices();
        }

        public void SendData(byte[] data)
        {
            using (SocketAsyncEventArgs args = new SocketAsyncEventArgs())
            {
                args.SetBuffer(data, 0, data.Length);
                ClientSocket.SendAsync(args);
            }
        }

        public void SendData(string data)
        {
            byte[] buffer = encoder.ConvertToByteArray(data);
            ClientSocket.Send(buffer);
        }

        internal SocketRequest(IController controller,
             string action, List<RequestParameter> parameters, Socket clientSocket)
        {
            Controller = controller;
            Action = action;
            requestParameters = parameters;
            SetClientSocket(clientSocket);
            InitializeServices();
        }

        private void InitializeServices()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            logger = manager.GetService<ILoggingService>();
            encoder = manager.GetService<IEncodingConverterService>();
            controllerManager = manager.GetService<IControllerManager>();
            coreServer = manager.GetService<ICoreServerService>("realserver");
            responseStorage = manager.GetService<IMemoryResponseStorage>();
        }
        #endregion

        #region Accessibility
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

        public void Fill(string controllerName, string action,
            IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, List<RequestParameter> parameters)
        {
            if (Controller != null)
                throw new Exception("It is not possible to apply changes to a requisition after it has been created.");

            RequestBody mockBody = new RequestBody();
            mockBody.Action = action;
            mockBody.Controller = controllerName;
            mockBody.Parameters = parameters;

            Controller = controllerManager.InstantiateController(controllerName, mockBody);
            Action = action;

            this.requestParameters = parameters;
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
        }

        internal void ProcessResponse(ActionResult result, Socket socket,
          string sessionStorageId)
        {
            ResponseStorageId = sessionStorageId;
            ComputeStatisticks(result);
            SetClientSocket(socket);
            ProcessResponse(result);
        }

        internal void SetClientSocket(Socket socket)
        {
            ClientSocket = socket;
            LocalEndPoint = (ClientSocket.LocalEndPoint as IPEndPoint);
            RemoteEndPoint = (ClientSocket.RemoteEndPoint as IPEndPoint);
        }

        internal void AddParameter(RequestParameter parameter)
        {
            requestParameters.Add(parameter);
        }

        /// <summary>
        /// Calculates the consumption of the action execution on the server and returns it in the response body
        /// </summary>
        /// <param name="result">response result</param>
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
                result.ServerMessage = msg;

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
        #endregion

        #region Network response
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
        #endregion
    }
}
