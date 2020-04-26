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
    internal class ClientConfiguration
    {
        public string Server { get; private set; }
        public int Port { get; private set; }
        public Encoding Encoding { get; private set; }
        public int PacketSize { get; private set; }
        public int MaxAttempts { get; private set; }

        public ClientConfiguration(string server, int port,
            Encoding encoding, int packetSize, int maxAttempts)
        {
            Server = server;
            Port = port;
            Encoding = encoding;
            PacketSize = packetSize;
            MaxAttempts = maxAttempts;
        }
    }

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

        private static ClientConfiguration staticConf;

        /// <summary>
        /// Defines a global, static configuration for any future connections needed by this client
        /// </summary>
        /// <param name="server">Server network address</param>
        /// <param name="port">Server port</param>
        /// <param name="encoding">Server encoding</param>
        /// <param name="packetSize">Packet/buffer size. The SAME size used on the server must be defined</param>
        /// <param name="maxAttempts">Maximum connection attempts</param>
        public static void Configure(string server, int port,
            Encoding encoding, int packetSize = 1024 * 100, int maxAttempts = 10)
        {
            staticConf = new ClientConfiguration(
                server,
                port,
                encoding,
                packetSize,
                maxAttempts);
        }

        /// <summary>
        /// The static configuration of the client has not been defined. 
        /// Invoke the "Configure()" method before using this constructor
        /// </summary>
        public Client()
        {
            if (staticConf == null)
                throw new Exception("");
            ConnectToServer(staticConf.Server,
                staticConf.Port, staticConf.Encoding,
                staticConf.PacketSize, staticConf.MaxAttempts);
        }

        /// <summary>
        /// Use this constructor when you need to connect to different servers at different points in your application
        /// </summary>
        /// <param name="server">Server network address</param>
        /// <param name="port">Server port</param>
        /// <param name="encoding">Server encoding</param>
        /// <param name="packetSize">Packet/buffer size. The SAME size used on the server must be defined</param>
        /// <param name="maxAttempts">Maximum connection attempts</param>
        public Client(string server, int port,
            Encoding encoding, int packetSize = 1024 * 100, int maxAttempts = 10)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectToServer(server, port, encoding, packetSize, maxAttempts);
        }

        private void ConnectToServer(string server,
            int port, Encoding encoding, int byteBuffer = 4098 * 50, int maxAttempts = 10)
        {
            int attempts = 0;
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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

        /// <summary>
        /// Retrieves the file returned by an action on the server
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///  Gets the result of a request on the server
        /// </summary>
        /// <param name="T">Defines the type of object to convert from the json returned by the action on the server</param>
        /// <returns></returns>
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

        /// <summary>
        /// Send a request to connected server
        /// </summary>
        /// <param name="body">Request body and parameters</param>
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

        /// <summary>
        /// Gets the response to a request made on the connected server. Conversion of objects are not handled here
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Closes current connection on server
        /// </summary>
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
            thisStr = "Disconnected";
        }
    }
}
