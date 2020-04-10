/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
