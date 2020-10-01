using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    public class SocketClientSettings
    {
        public string Server { get; private set; }
        public int Port { get; private set; }
        public Encoding Encoding { get; private set; }
        public int MaxAttempts { get; private set; }
        public JsonSerializerSettings SerializerSettings { get; private set; }

        public int Timeout { get; }
        [Obsolete("")]
        public int BufferSize { get; private set; }

        internal SocketClientSettings()
        {

        }

        public SocketClientSettings(string server, int port,
            Encoding encoding, int maxAttempts = 3,
            int timeout = 5000,
            JsonSerializerSettings jsonSettings = null)
        {
            Server = server;
            Port = port;
            Encoding = encoding;
            MaxAttempts = maxAttempts;
            Timeout = timeout;

            if (jsonSettings != null)
            {
                SerializerSettings = new JsonSerializerSettings();
                foreach (PropertyInfo prop in SerializerSettings.GetType().GetProperties())
                {
                    try
                    {
                        var value = jsonSettings.GetType().GetProperty(prop.Name).GetValue(jsonSettings);
                        if (value == null)
                            continue;
                        prop.SetValue(SerializerSettings, value);
                    }
                    catch { }
                }
            }
        }


        [Obsolete]
        public SocketClientSettings(string server, int port,
            Encoding encoding, int packetSize, int maxAttempts,
            int receiveTimeOut, JsonSerializerSettings settings)
        {
            Server = server;
            Port = port;
            Encoding = encoding;
            BufferSize = packetSize;
            MaxAttempts = maxAttempts;
            Timeout = receiveTimeOut;

            SerializerSettings = new JsonSerializerSettings();
            foreach (PropertyInfo prop in SerializerSettings.GetType().GetProperties())
            {
                try
                {
                    var value = settings.GetType().GetProperty(prop.Name).GetValue(settings);
                    if (value == null)
                        continue;
                    prop.SetValue(SerializerSettings, value);
                }
                catch { }
            }
        }
    }
}
