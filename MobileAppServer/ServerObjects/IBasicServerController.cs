using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.ServerObjects
{
    public interface IBasicServerController
    {
        ActionResult RunAction(string receivedData);
    }
}
