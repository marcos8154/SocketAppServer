using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    internal class ServerInfoServiceImpl : IServerInformationService
    {
        ISocketClientConnection connection;

        public ServerInfoServiceImpl(ISocketClientConnection connection)
        {
            this.connection = connection;
        }

        public double ServerAverageCPUUsage(int lastMinutes = 3)
        {
            connection.SendRequest("ServerInfoController", "AverageCPUUsage", new
            {
                lastMinutes
            });

            return connection.GetResult<double>();
        }

        public double ServerAverageMemoryUsage(int lastMinutes = 3)
        {
            connection.SendRequest("ServerInfoController", "AverageMemoryUsage", new
            {
                lastMinutes
            });

            return connection.GetResult<double>();
        }

        public int RequestsErrorsCount(string controllerName, string actionName)
        {
            connection.SendRequest("ServerInfoController", "RequestsErrorsCount", new
            {
                controllerName,
                actionName
            });

            return connection.GetResult<int>();
        }

        public int RequestsSuccessCount(string controllerName, string actionName)
        {
            connection.SendRequest("ServerInfoController", "RequestsSuccessCount", new
            {
                controllerName,
                actionName
            });

            return connection.GetResult<int>();
        }

        public long GetCurrentThreadsCount()
        {
            connection.SendRequest("ServerInfoController", "GetCurrentThreadsCount");
            return  connection.GetResult<long>();
        }

        public string[] GetActionParameters(string controller, string action)
        {
            connection.SendRequest("ServerInfoController", "GetActionParameters", new
            {
                controller,
                action
            });

            return connection.GetResult<string[]>();
        }

        public ServerInfo GetFullServerInfo()
        {
            connection.SendRequest("ServerInfoController", "FullServerInfo");
            return connection.GetResult<ServerInfo>();
        }
    }
}
