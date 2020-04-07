using System.Net;
using System.Net.Sockets;

namespace MobileAppServer.CoreServices.CoreServer
{
    public class SocketSession
    {
        internal Socket ClientSocket { get; private set; }

        public string ClienteRemoteAddress
        {
            get
            {
                if (ClientSocket == null)
                    return string.Empty;
                if (SessionClosed)
                    return string.Empty;

                IPEndPoint remoteIpEndPoint = ClientSocket.RemoteEndPoint as IPEndPoint;
                return remoteIpEndPoint.Address.ToString();
            }
        }

        internal byte[] SessionStorage { get; private set; }

        public SocketSession(Socket socket, int bufferSize)
        {
            ClientSocket = socket;
            SessionStorage = new byte[bufferSize];
        }

        public bool SessionClosed { get; private set; }

        internal void Close()
        {
            if (SessionClosed)
                return;

            ClientSocket.Close();
            ClientSocket.Dispose();
            ClientSocket = null;
            SessionStorage = null;
            SessionClosed = true;
        }
    }
}
