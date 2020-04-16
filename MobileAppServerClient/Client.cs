using SocketAppServerClient.ClientUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    public class Client
    {
        private string thisStr = null;

        public override string ToString()
        {
            return thisStr;
        }

        public string Server { get; private set; }
        public int Port { get; private set; }
        public Encoding Encoding { get; private set; }
        public int ByteBuffer { get; private set; }
        private Socket clientSocket = null;

        public Client(string server, int port,
            Encoding encoding, int packetSize, int maxAttempts = 10)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectToServer(server, port, encoding, packetSize, maxAttempts);
        }

        private void ConnectToServer(string server,
            int port, Encoding encoding, int byteBuffer = 4098 * 50, int maxAttempts = 10)
        {
            int attempts = 0;
            while (!clientSocket.Connected)
            {
                if (attempts >= maxAttempts)
                    throw new Exception("Maximum number of attempts exceeded");

                attempts++;
                clientSocket.Connect(server, port);
            }

            Server = server;
            Port = port;
            Encoding = encoding;
            ByteBuffer = byteBuffer;
            thisStr = $"Connected on {server}:{port}";
        }

        public ServerResponse Response { get; private set; }

        public byte[] GetResultFile()
        {
            try
            {
                byte[] responseBytes = ReceiveBytes();
                Close();
                return responseBytes;
            }
            catch
            {
                Close();
                return null;
            }
        }

        private OperationResult GetResultInternal(string responseContent)
        {
            using (StringReader sr = new StringReader(responseContent))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    OperationResult result = js.Deserialize<OperationResult>(jr);
                    return result;
                }
            }
        }

        private object GetEntityObjectInternal(string entityJson,
            Type entityType = null)
        {
            using (StringReader sr = new StringReader(entityJson))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    object entityResult = js.Deserialize(jr, entityType);
                    return entityResult;
                }
            }
        }

        [Obsolete]
        public OperationResult GetResult(Type entityType = null)
        {
            try
            {
                ServerResponse response = ReadResponse();
                if (response.Status == 500)
                    throw new Exception(response.Message);

                Response = response;
                OperationResult result = GetResultInternal(response.Content.ToString());

                if (entityType != null)
                {
                    try
                    {
                        string entityJson = result.Entity.ToString();
                        result.Entity = GetEntityObjectInternal(entityJson, entityType);
                    }
                    finally { }
                }
                Close();
                return result;
            }
            catch
            {
                Close();
                throw;
            }
        }

        public OperationResult GetResult<T>()
            where T : class
        {
            try
            {
                ServerResponse response = ReadResponse();
                if (response.Status == 500)
                    throw new Exception(response.Message);

                Response = response;
                OperationResult result = GetResultInternal(response.Content.ToString());

                try
                {
                    string entityJson = result.Entity.ToString();
                    result.Entity = GetEntityObjectInternal(entityJson, typeof(T));
                }
                finally { }

                Close();
                return result;
            }
            catch
            {
                Close();
                throw;
            }
        }


        public void SendRequest(RequestBody body)
        {
            string commandRequest = JsonConvert.SerializeObject(body);
            byte[] buffer = Encoding.GetBytes(commandRequest);
            clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private byte[] ReceiveBytes()
        {
            byte[] buffer = new byte[ByteBuffer];
            int received = clientSocket.Receive(buffer, SocketFlags.None);
            var data = new byte[received];
            Array.Copy(buffer, data, received);

            return data;
        }

        private ServerResponse ReadResponseInternal(string responseText)
        {
            using (StringReader sr = new StringReader(responseText))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    ServerResponse response = js.Deserialize<ServerResponse>(jr);
                    return response;
                }
            }
        }

        public ServerResponse ReadResponse()
        {
            try
            {
                string responseText = Encoding.GetString(ReceiveBytes());
                responseText = responseText.Replace("\t", "");

                ServerResponse response = ReadResponseInternal(responseText);
                if (response.Type == 1)
                    if (response.FileState == "BOF")
                        response.Content = Encoding.GetBytes(response.Content.ToString()); ;

                return response;
            }
            catch (Exception ex)
            {
                return new ServerResponse(500, ex.Message, "", 0);
            }
        }

        public void Close()
        {
            if (clientSocket == null)
                return;
            try
            {
                clientSocket.Close();
                clientSocket.Dispose();
                clientSocket = null;
            }
            catch { }
        }
    }
}
