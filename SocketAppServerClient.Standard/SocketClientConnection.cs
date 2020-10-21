using Newtonsoft.Json;
using SocketAppServerClient.ClientUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    internal class SocketClientConnection : ISocketClientConnection, IDisposable
    {
        private SocketClientSettings configuration;
        private Socket clientSocket;
        private ServerResponseHelper responseHelper;
        private string thisStr;
        private AutoResetEvent connectionEvent;

        private bool beginConnectFailed = false;
        private string beginConnectErrorMessage = null;

        internal SocketClientConnection(SocketClientSettings config)
        {
            configuration = config;
            responseHelper = new ServerResponseHelper(config);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            if (clientSocket.Connected)
                return;

            beginConnectFailed = false;
            beginConnectErrorMessage = null;

            connectionEvent = new AutoResetEvent(false);
            int attempts = 0;
            while (!clientSocket.Connected)
            {
                if (attempts >= configuration.MaxAttempts)
                    throw new Exception("Maximum number of attempts exceeded");
                attempts++;

                clientSocket.SendTimeout = configuration.Timeout;
                clientSocket.ReceiveTimeout = configuration.Timeout;

                clientSocket.BeginConnect(configuration.Server, configuration.Port, OnTcpClientConnected, clientSocket);
                connectionEvent.WaitOne(configuration.Timeout);

                if (beginConnectFailed)
                    throw new Exception(beginConnectErrorMessage);
            }

            if (!clientSocket.Connected)
                throw new Exception("Connection fail");
            thisStr = $"SocketClient for .NET Standard. Connected to {configuration.Server}:{configuration.Port}";
        }

        private void DisconnectServer()
        {
            if (clientSocket.Connected)
                clientSocket.Disconnect(true);
        }

        private void OnTcpClientConnected(IAsyncResult asyncResult)
        {
            try
            {
                clientSocket.EndConnect(asyncResult);
            }
            catch (Exception ex)
            {
                beginConnectFailed = true;
                beginConnectErrorMessage = ex.Message;
            }
            connectionEvent.Set();
        }

        public void SendRequest(RequestBody body)
        {
            ConnectToServer();
            StringBuilder sb = new StringBuilder(1000);
            using (StringWriter sw = new StringWriter(sb))
            {
                using (JsonTextWriter jw = new JsonTextWriter(sw))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.ApplyCustomSettings(configuration.SerializerSettings);
                    js.Serialize(jw, body);
                    string commandRequest = sb.ToString();
                    byte[] buffer = configuration.Encoding.GetBytes(commandRequest);
                    clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }
            }
        }

        public void SendRequest(string controller, string action, object param = null)
        {
            RequestBody rb = RequestBody.Create(controller, action);
            if (param != null)
                foreach (PropertyInfo info in param.GetType().GetProperties())
                    rb.AddParameter(info.Name, info.GetValue(param));
            SendRequest(rb);
        }

        public ServerResponse ReadResponse()
        {
            try
            {
                var buffer = responseHelper.ReceiveBytes(ref clientSocket);
                string responseText = configuration.Encoding.GetString(buffer, 0, buffer.Length);

                if (responseText.Contains("\t"))
                    responseText = responseText.Replace("\t", "");

                ServerResponse response = responseHelper.ReadResponseInternal(responseText);
                if (response.Type == 1)
                    if (response.FileState == "BOF")
                        response.Content = configuration.Encoding.GetBytes(response.Content.ToString()); ;

                return response;
            }
            catch (Exception ex)
            {
                return new ServerResponse(500, ex.Message, "", 0);
            }
            finally
            {
                DisconnectServer();
            }
        }

        public OperationResult GetResult()
        {
            ServerResponse response = ReadResponse();
            if (response.Status == 500)
                throw new Exception(response.Message);

            OperationResult result = responseHelper.GetResultInternal(response.Content.ToString());
            return result;
        }

        public T GetResultObject<T>()
        {
            string entityTypeName = string.Empty;
            try
            {
                ServerResponse response = ReadResponse();
                if (response.Status == 500)
                    throw new Exception(response.Message);

                OperationResult result = responseHelper.GetResultInternal(response.Content.ToString());

                if (!string.IsNullOrEmpty(result.Entity + ""))
                {
                    string entityJson = result.Entity.ToString();
                    if (entityJson.Contains("{") && entityJson.Contains("}"))
                        result.Entity = responseHelper.GetEntityObjectInternal(entityJson, typeof(T));
                }

                if (result.Entity == null)
                    return default(T);
                else
                {
                    entityTypeName = result.Entity.GetType().Name;
                    if (entityTypeName.Equals("JArray"))
                        return default(T);
                    else
                        return (T)result.Entity;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}\nReturned entity Type: {entityTypeName}", ex);
            }
            finally
            {
                DisconnectServer();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                if (clientSocket.Connected)
                    clientSocket.Close();
                clientSocket.Dispose();

                clientSocket = null;
                responseHelper = null;
            }
        }

        public override string ToString()
        {
            return thisStr;
        }

        public IServerInformationService GetServerInfo()
        {
            return new ServerInfoServiceImpl(this);
        }


    }
}
