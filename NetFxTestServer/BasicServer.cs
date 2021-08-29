using SocketAppServer.CoreServices;
using SocketAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFxTestServer
{
    public class BasicServer : IBasicServerController
    {
        public object RunAction(string receivedData, SocketRequest request)
        {
            request.SendData("m                                                                               ");


            return @"<STX>
<L>
<ETX>";
        }
    }
}
