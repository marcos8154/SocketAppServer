using SocketAppServerClient.ClientUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace SocketAppServerClient
{
    [Obsolete("This class will no longer be supported. Instead, use the new ISocketClientConnection interface, obtained through SocketConnectionFactory.GetConnection()")]
    public class Client
    {
        private string thisStr = null;

        public override string ToString()
        {
            return thisStr;
        }

        private string _chunkeResultStorageId;
        public string Server { get; private set; }
        public int Port { get; private set; }
        public Encoding Encoding { get; private set; }
        public int BufferSize { get; private set; }
        public JsonSerializerSettings SerializerSettings { get; private set; }
        private Socket clientSocket = null;

        private static SocketClientSettings staticConf;

        /// <summary>
        /// Defines a global, static configuration for any future connections needed by this client
        /// </summary>
        /// <param name="server">Server network address</param>
        /// <param name="port">Server port</param>
        /// <param name="encoding">Server encoding</param>
        /// <param name="packetSize">Packet/buffer size. The SAME size used on the server must be defined</param>
        /// <param name="maxAttempts">Maximum connection attempts</param>
        /// <param name="receiveTimeOut">Maximum waiting time (in milliseconds) for receiving request data</param>
        /// <param name="serializerSettings">Maximum waiting time (in milliseconds) for receiving request data</param>
        public static void Configure(string server, int port,
            Encoding encoding, int packetSize = 1024 * 100, int maxAttempts = 10,
            int receiveTimeOut = 0,
            JsonSerializerSettings serializerSettings = null)
        {
            staticConf = new SocketClientSettings(
                server,
                port,
                encoding,
                packetSize,
                maxAttempts,
                receiveTimeOut,
                serializerSettings);
        }

        public static SocketClientSettings GetConfiguration()
        {
            if (staticConf == null)
                return null;

            SocketClientSettings conf = new SocketClientSettings();
            foreach (PropertyInfo prop in conf.GetType().GetProperties())
            {
                try
                {
                    var value = staticConf.GetType().GetProperty(prop.Name).GetValue(staticConf);
                    if (value == null)
                        continue;
                    prop.SetValue(conf, value);
                }
                catch { }
            }
            return conf;
        }

        /// <summary>
        /// The static configuration of the client has not been defined. 
        /// Invoke the "Configure()" method before using this constructor
        /// </summary>
        public Client()
        {
            if (staticConf == null)
                throw new Exception("Client Configuration has not been defined");
            ConnectToServer(staticConf.Server,
                staticConf.Port, staticConf.Encoding,
                staticConf.BufferSize, staticConf.MaxAttempts,
                staticConf.SerializerSettings);
        }

        /// <summary>
        /// Use this constructor when you need to connect to different servers at different points in your application
        /// </summary>
        /// <param name="server">Server network address</param>
        /// <param name="port">Server port</param>
        /// <param name="encoding">Server encoding</param>
        /// <param name="packetSize">Packet/buffer size. The SAME size used on the server must be defined</param>
        /// <param name="maxAttempts">Maximum connection attempts</param>
        /// <param name="receiveTimeOut">Maximum waiting time (in milliseconds) for receiving request data</param>
        public Client(string server, int port,
            Encoding encoding, int packetSize = 1024 * 100, int maxAttempts = 10,
            int receiveTimeOut = 0,
            JsonSerializerSettings serializerSettings = null)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectToServer(server, port, encoding, packetSize, maxAttempts, serializerSettings);
            ReceiveTimeOut = receiveTimeOut;
        }

        private void ConnectToServer(string server,
            int port, Encoding encoding, int byteBuffer = 4098 * 50, int maxAttempts = 10,
            JsonSerializerSettings serializerSettings = null)
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
            BufferSize = byteBuffer;

            if (serializerSettings != null)
            {
                SerializerSettings = new JsonSerializerSettings();
                foreach (PropertyInfo prop in SerializerSettings.GetType().GetProperties())
                {
                    try
                    {
                        var value = serializerSettings.GetType().GetProperty(prop.Name).GetValue(serializerSettings);
                        if (value == null)
                            continue;
                        prop.SetValue(SerializerSettings, value);
                    }
                    catch { }
                }
            }

            thisStr = $"SocketClient for .NET Standard. Connected on {server}:{port}";
        }

        public ServerResponse Response { get; private set; }
        public int ReceiveTimeOut { get; }

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
                    js.ApplyCustomSettings(SerializerSettings);
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
                    js.ApplyCustomSettings(SerializerSettings);
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
                OperationResult result = (response.Content == null
                    ? null
                    : GetResultInternal(response.Content.ToString()));

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
        /// Gets the chunked response from the server. 
        /// It should be used in actions that tend to return great responses. 
        /// In this case, the reading will be made progressively through
        /// the length informed
        /// </summary>
        /// <param name="T">Defines the type of object to convert from the json returned by the action on the server</param>
        /// <param name="chunkLength">Size of each part of the response</param>
        /// <returns></returns>
        public OperationResult GetChunkedResult(int chunkLength)
        {
            ChunkedResponseReader chunkedResponse = new ChunkedResponseReader(_chunkeResultStorageId,
                chunkLength, this);
            return chunkedResponse.GetResult();
        }

        /// <summary>
        ///  Gets the result of a request on the server
        /// </summary>
        /// <param name="T">Defines the type of object to convert from the json returned by the action on the server</param>
        /// <returns></returns>
        public T GetResult<T>()
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
                catch { }

                Close();
                return (T)result.Entity;
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
            _chunkeResultStorageId = body.InTo;
            string commandRequest = JsonConvert.SerializeObject(body, SerializerSettings);
            byte[] buffer = Encoding.GetBytes(commandRequest);
            clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private byte[] ReceiveBytes()
        {
            var readEvent = new AutoResetEvent(false);
            var buffer = new byte[BufferSize];
            var totalRecieved = 0;
            int receivedChunk = 0;
            int dynamicTimeOut = 0;
            do
            {
                var recieveArgs = new SocketAsyncEventArgs()
                {
                    UserToken = readEvent
                };
                recieveArgs.SetBuffer(buffer, totalRecieved, BufferSize - totalRecieved);
                recieveArgs.Completed += recieveArgs_Completed;
                clientSocket.ReceiveAsync(recieveArgs);

                if (ReceiveTimeOut == 0)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    if (dynamicTimeOut == 0)
                        readEvent.WaitOne();
                    else
                        readEvent.WaitOne(dynamicTimeOut);

                    sw.Stop();
                    dynamicTimeOut = (int)sw.ElapsedMilliseconds;
                    if (dynamicTimeOut > 1000)
                        dynamicTimeOut = 1000;
                }
                else readEvent.WaitOne(ReceiveTimeOut);

                if (recieveArgs.BytesTransferred <= 0)
                {
                    if (recieveArgs.SocketError != SocketError.Success)
                        throw new Exception("Unexpected Disconnect");
                    return buffer;
                }

                receivedChunk = recieveArgs.BytesTransferred;
                totalRecieved += receivedChunk;

            } while (receivedChunk > 0);
            return buffer;
        }

        void recieveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            var are = (AutoResetEvent)e.UserToken;
            are.Set();
        }

        private ServerResponse ReadResponseInternal(string responseText)
        {
            using (StringReader sr = new StringReader(responseText))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.ApplyCustomSettings(SerializerSettings);
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
            string responseText = null;
            try
            {
                var buffer = ReceiveBytes();
                responseText = Encoding.GetString(buffer, 0, buffer.Length);

                buffer = null;
                if (responseText.Contains("\t"))
                    responseText = responseText.Replace("\t", "");

                ServerResponse response = ReadResponseInternal(responseText);
                if (response.Type == 1)
                    if (response.FileState == "BOF")
                        response.Content = Encoding.GetBytes(response.Content.ToString()); ;

                responseText = null;
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
                if (clientSocket.Connected)
                    clientSocket.Close();
                clientSocket.Dispose();
                clientSocket = null;
            }
            catch { }
            thisStr = "Disconnected";
        }
    }
}
