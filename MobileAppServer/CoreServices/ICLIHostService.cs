using SocketAppServer.CoreServices.CLIHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices
{
    public interface ICLIHostService
    {
        void RegisterCLICommand(string commandText, string commandDescription, ICLIClient cliClient);

        IReadOnlyCollection<CLICommand> RegisteredCommands();

        CLICommand GetCommand(string commandText);

        void SetCLIBusy(bool isBusy);

        bool IsCLIBusy();
    }
}
