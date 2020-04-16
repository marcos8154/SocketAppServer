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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.CoreServer
{
    public class ServerConfiguration
    {
        public Encoding ServerEncoding { get; private set; }
        public int BufferSize { get; private set; }
        public int Port { get; private set; }
        public bool IsSingleThreaded { get; private set; }
        public int MaxThreadsCount { get; private set; }
        public bool IsConsoleApplication { get; }

        /// <summary>
        /// Provides parameters for configuring the current server
        /// </summary>
        /// <param name="serverEncoding">Encoding of server input and output data</param>
        /// <param name="port">Server listening port</param>
        /// <param name="bufferSize">Maximum size of memory allocated to store the data for each request. It does not mean that each process will necessarily allocate this size in bytes, but rather the maximum possible buffer to be allocated. At the end of each request, the buffer will be dispensed by the server's memory and will be available for collection from the GC</param>
        /// <param name="isSingleThreaded">Indicates whether the server will use only one Thread to fulfill requests. If enabled, a queue will be created for requesting clients</param>
        /// <param name="maxThreadsCount">Maximum number of simultaneous tasks possible to be attended by the server. Note that this number is related to the BufferSize and each process will allocate its own buffer in memory. If the maximum number is exceeded and the server is busy, clients that have not yet been served will join a queue and be served according to the order of arrival and as current processes are released</param>
        /// <param name="isConsoleApplication">If this application is a ConsoleApp, the server will take care of holding the process in memory. If your application is a DesktopApp or WebApp, set this parameter to false</param>
        public ServerConfiguration(Encoding serverEncoding,
            int port, int bufferSize = 10000000,
            bool isSingleThreaded = false, int maxThreadsCount = 100,
            bool isConsoleApplication = true)
        {
            ServerEncoding = serverEncoding;
            BufferSize = bufferSize;
            Port = port;
            IsSingleThreaded = isSingleThreaded;
            MaxThreadsCount = maxThreadsCount;
            IsConsoleApplication = isConsoleApplication;
        }

        public ServerConfiguration()
        {
            //SocketAPI.Server
            //SocketAppServer
            //SocketAPI.NET
            //Socket.API
        }
    }
}
