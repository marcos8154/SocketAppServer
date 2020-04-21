using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.CLIHost
{
    public class CLICommand
    {
        public string CommandText { get; private set; }
        public string CommandDescription { get; private set; }

        public ICLIClient ExecutorClient { get; private set; }

        public CLICommand(string commandText, string commandDescription, ICLIClient executorClient)
        {
            CommandText = commandText;
            CommandDescription = commandDescription;
            ExecutorClient = executorClient;
        }
    }
}
