using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.CLIHost
{
    internal class CLIHostServiceImpl : ICLIHostService
    {
        private List<CLICommand> commands;
        public CLIHostServiceImpl()
        {
            commands = new List<CLICommand>();
        }

        public CLICommand GetCommand(string commandText)
        {
            return commands.FirstOrDefault(c => c.CommandText.Equals(commandText));
        }

        public void RegisterCLICommand(string commandText, string commandDescription, ICLIClient cliClient)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new Exception("CommandText is null");
            if (cliClient == null)
                throw new Exception("Client is null");
            if (GetCommand(commandText) != null)
                throw new Exception("This command is already in use by another CLI client");
            if (RegisteredCommands().Any(c => c.ExecutorClient.GetType() == cliClient.GetType()))
                throw new Exception($"CLI client type '{cliClient.GetType().FullName}' is already in use");

            commands.Add(new CLICommand(commandText, commandDescription, cliClient));
        }

        public IReadOnlyCollection<CLICommand> RegisteredCommands()
        {
            return commands.AsReadOnly();
        }

        private bool isCliBusy = false;
        public void SetCLIBusy(bool isBusy)
        {
            isCliBusy = isBusy;
        }

        public bool IsCLIBusy()
        {
            return isCliBusy;
        }

    }
}
