using SocketAppServer.CoreServices.CLIHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultTestServer
{
    public class MySimpleCLICommand : ICLIClient
    {
        public void Activate()
        {
            Console.WriteLine(" YES! Im successfully activated!");
        }
    }
}
