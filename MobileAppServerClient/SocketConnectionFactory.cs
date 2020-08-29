using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    public sealed class SocketConnectionFactory
    {
        private static SocketClientSettings defaultSettings;
        public static void SetDefaultSettings(SocketClientSettings settings)
        {
            if (defaultSettings != null)
                throw new InvalidOperationException("Already applied");
            defaultSettings = settings;
        }

        public static ISocketClientConnection GetConnection()
        {
            if (defaultSettings == null)
                throw new InvalidOperationException("The static connection configuration has not been set. \nInvoke the SetDefaultSettings method or obtain the connection through GetConnection (SocketClientSettings)");
            return new SocketClientConnection(defaultSettings);
        }

        public static ISocketClientConnection GetConnection(SocketClientSettings settings)
        {
            if (settings == null)
                throw new InvalidOperationException("Settings cannot be null");
            return new SocketClientConnection(settings);
        }
    }
}
