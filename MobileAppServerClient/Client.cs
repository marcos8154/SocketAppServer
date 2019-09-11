using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServerClient
{
    class Config
    {
        public string SERVER { get; set; }
        public string PORT { get; set; }
        public string ENCODING { get; set; }
        public string MAXPACKETSIZE { get; set; }
    }

    public class Client
    {
        public static void Configure(string server, int port, 
            int maxPacketSize = 4096)
        {
            try
            {
                string conf = $@"SERVER={server}
PORT={port}
ENCODING=UTF-8
MAXPACKETSIZE={maxPacketSize}";
                File.WriteAllText(@".\MobileAppServerClient.conf", conf);
            }
            catch { }
        }

        public string Server { get; set; }
        public int Port { get; set; }
        public Encoding Encoding { get; set; }
        public int ByteBuffer { get; set; }


        private Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Config conf = new Config();

        public Client()
        {
        
            string[] configs = File.ReadAllLines(@".\MobileAppServerClient.conf");
            for (int i = 0; i < configs.Length; i++)
            {
                string propertyName = configs[i].Substring(0, configs[i].LastIndexOf("="));
                string propertyValue = configs[i].Substring(configs[i].LastIndexOf("=") + 1);
                conf.GetType().GetProperty(propertyName)
                    .SetValue(conf, propertyValue, null);
            } 

            ConnectToServer(conf.SERVER, int.Parse(conf.PORT),
                Encoding.GetEncoding(conf.ENCODING), int.Parse(conf.MAXPACKETSIZE));
     
        }

        public void ConnectToServer(string server,
            int port, Encoding encoding, int byteBuffer = 4098 * 50, int maxAttempts = 10)
        {
            int attempts = 0;
            while (!ClientSocket.Connected)
            {
                if (attempts >= maxAttempts)
                    throw new Exception("Número máximo de tentativas excedidas");

                attempts++;
                ClientSocket.Connect(server, port);
            }

            Server = server;
            Port = port;
            Encoding = encoding;
            ByteBuffer = byteBuffer;
        }

        public OperationResult GetResult(Type entityType = null)
        {
            ServerResponse response = ReadResponse();
            if (response.Status == 500)
                throw new Exception(response.Message);

            OperationResult result = (OperationResult)JsonConvert.DeserializeObject(response.Content.ToString(), typeof(OperationResult));

            if (entityType != null)
            {
                try
                {
                    string entityJson = result.Entity.ToString();
                    Object entity = JsonConvert.DeserializeObject(entityJson, entityType);
                    result.Entity = entity;
                }
                catch (Exception ex)
                {

                }
            }
            Close();
            return result;
        }

        public void SendRequest(RequestBody body)
        {
            string commandRequest = JsonConvert.SerializeObject(body);

            byte[] buffer = Encoding.GetBytes(commandRequest);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private byte[] ReceiveBytes()
        {
            byte[] buffer = new byte[ByteBuffer];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            var data = new byte[received];
            Array.Copy(buffer, data, received);

            return data;
        }

        public ServerResponse ReadResponse()
        {
            try
            {
                string responseText = Encoding.GetString(ReceiveBytes());
                if (!responseText.EndsWith("}"))
                    responseText += "}";
                responseText = responseText.Replace("\t", "");

                ServerResponse response = JsonConvert.DeserializeObject<ServerResponse>(responseText);
                if (response.Type == 1)
                {
                    if (response.FileState == "BOF")
                        response.Content = ReceiveBytes();

                    response.Message = Encoding.ASCII.GetString((byte[])response.Content);
                    File.WriteAllBytes(@"C:\Temp\img2.png", (byte[])response.Content);
                }
                return response;
            }
            catch (Exception ex)
            {
                return new ServerResponse(500, ex.Message, "", 0);
            }
        }

        public void Close()
        {
            try
            {
                ClientSocket.Close();
                ClientSocket.Dispose();
                ClientSocket = null;
            }
            catch { }
        }
    }
}
