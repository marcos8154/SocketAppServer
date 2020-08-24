using Newtonsoft.Json;
using SocketAppServerClient.ClientUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    internal class ServerResponseHelper
    {
        private ClientConfiguration configuration;
        private int receiveTimeout;

        public ServerResponseHelper(ClientConfiguration config)
        {
            configuration = config;
        }


        public byte[] ReceiveBytes(ref Socket clientSocket)
        {
            var readEvent = new AutoResetEvent(false);
            var buffer = new byte[configuration.BufferSize];
            var totalRecieved = 0;
            int receivedChunk = 0;
            int dynamicTimeOut = 0;
            do
            {
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs()
                {
                    UserToken = readEvent
                };
                receiveArgs.SetBuffer(buffer, totalRecieved, configuration.BufferSize - totalRecieved);
                receiveArgs.Completed += recieveArgs_Completed;
                clientSocket.ReceiveAsync(receiveArgs);

                if (receiveTimeout == 0)
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
                else readEvent.WaitOne(receiveTimeout);

                if (receiveArgs.BytesTransferred <= 0)
                {
                    receiveArgs.Completed -= recieveArgs_Completed;
                    if (receiveArgs.SocketError != SocketError.Success)
                        throw new Exception("Unexpected Disconnect");

                    int count = buffer.Count(b => b > 0);
                    byte[] result = new byte[count];
                    Array.Copy(buffer, result, count);
                    return result;
                }

                receivedChunk = receiveArgs.BytesTransferred;
                totalRecieved += receivedChunk;
                receiveArgs.Completed -= recieveArgs_Completed;

            } while (receivedChunk > 0);

            return buffer;
        }

        void recieveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.ConnectSocket.EndReceive();
            var are = (AutoResetEvent)e.UserToken;
            are.Set();
        }

        public object GetEntityObjectInternal(string entityJson,
    Type entityType = null)
        {
            using (StringReader sr = new StringReader(entityJson))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.ApplyCustomSettings(configuration.SerializerSettings);
                    object entityResult = js.Deserialize(jr, entityType);
                    return entityResult;
                }
            }
        }

        public OperationResult GetResultInternal(string responseContent)
        {
            using (StringReader sr = new StringReader(responseContent))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.ApplyCustomSettings(configuration.SerializerSettings);
                    OperationResult result = js.Deserialize<OperationResult>(jr);
                    return result;
                }
            }
        }

        public ServerResponse ReadResponseInternal(string responseText)
        {
            using (StringReader sr = new StringReader(responseText))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.ApplyCustomSettings(configuration.SerializerSettings);
                    ServerResponse response = js.Deserialize<ServerResponse>(jr);
                    return response;
                }
            }
        }
    }
}
