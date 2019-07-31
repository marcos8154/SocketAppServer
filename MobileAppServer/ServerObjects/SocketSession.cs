using System.Net.Sockets;

namespace MobileAppServer.ServerObjects
{
    internal class SocketSession
    {
        public Socket ClientSocket { get; private set; }

        public byte[] SessionStorage { get; private set; }

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
