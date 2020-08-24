using Newtonsoft.Json;
using SocketAppServerClient.ClientUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    public class SocketClientConnection : IDisposable
    {
        private int receiveTimeout;
        private ClientConfiguration configuration;
        private Socket clientSocket;
        private ServerResponseHelper responseHelper;
        private string thisStr;

        public SocketClientConnection(ClientConfiguration config)
        {
            configuration = config;
            responseHelper = new ServerResponseHelper(config);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectToServer();
        }

        private AutoResetEvent connectionEvent;

        private void ConnectToServer()
        {
            if (clientSocket.Connected)
                return;

            connectionEvent = new AutoResetEvent(false);
            int attempts = 0;
            while (!clientSocket.Connected)
            {
                if (attempts >= configuration.MaxAttempts)
                    throw new Exception("Maximum number of attempts exceeded");
                attempts++;
                //   clientSocket.Connect(configuration.Server, configuration.Port);

                clientSocket.BeginConnect(configuration.Server, configuration.Port, OnTcpClientConnected, clientSocket);
                connectionEvent.WaitOne();
            }
            thisStr = $"SocketClient for .NET 4.x. Connected on {configuration.Server}:{configuration.Port}";
        }

        private void OnTcpClientConnected(IAsyncResult asyncResult)
        {
            clientSocket.EndConnect(asyncResult);
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
                    js.Serialize(jw, body);
                    string commandRequest = sb.ToString();
                    byte[] buffer = configuration.Encoding.GetBytes(commandRequest);
                    clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }
            }
        }

        public T GetResult<T>() where T : class
        {
            try
            {
                ServerResponse response = ReadResponse();
                if (response.Status == 500)
                    throw new Exception(response.Message);

                //   Response = response;
                OperationResult result = responseHelper.GetResultInternal(response.Content.ToString());

                try
                {
                    string entityJson = result.Entity.ToString();
                    if (entityJson.Contains("{") && entityJson.Contains("}"))
                        result.Entity = responseHelper.GetEntityObjectInternal(entityJson, typeof(T));
                }
                catch
                {
                    return null;
                }

                return (T)result.Entity;
            }
            catch
            {
                return null;
            }
            finally
            {
                clientSocket.Disconnect(true);
            }
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
        }

        /// <summary>
        /// [OBSOLETE!!!] Gets the result of a request on the server
        /// </summary>
        /// <param name="entityType">Defines the type of object to convert from the json returned by the action on the server</param>
        /// <returns></returns>
        [Obsolete]
        public OperationResult GetResult(Type entityType = null)
        {
            try
            {
                ServerResponse response = ReadResponse();
                if (response.Status == 500)
                    throw new Exception(response.Message);

                //   Response = response;
                OperationResult result = (response.Content == null
                    ? null
                    : responseHelper.GetResultInternal(response.Content.ToString()));

                if (entityType != null)
                {
                    try
                    {
                        string entityJson = result.Entity.ToString();
                        if (entityJson.Contains("{") && entityJson.Contains("}"))
                            result.Entity = responseHelper.GetEntityObjectInternal(entityJson, entityType);
                    }
                    finally { }
                }

                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                clientSocket.Disconnect(true);
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
    }
}
