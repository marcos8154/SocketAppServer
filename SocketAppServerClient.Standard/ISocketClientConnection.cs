using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    public interface ISocketClientConnection : IDisposable
    {
        /// <summary>
        /// Send the request to the server using a RequestBody instance with the parameters
        /// </summary>
        /// <param name="body">Request body containing the target Controller and Action, as well as the parameters if any</param>
        void SendRequest(RequestBody body);

        /// <summary>
        /// Send the request to the server using an anonymous object as a parameter
        /// </summary>
        /// <param name="controller">Target controller</param>
        /// <param name="action">Target action</param>
        /// <param name="param">An anonymous object "new {...}" whose properties and their respective types are equivalent to action parameters</param>
        void SendRequest(string controller, string action, object param = null);

        /// <summary>
        /// Reads the server's gross response (return entities will not be deserialized)
        /// </summary>
        /// <returns></returns>
        ServerResponse ReadResponse();

        /// <summary>
        /// Gets the return from the server converted to an object (T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetResult<T>();

        /// <summary>
        /// Get an instance of the server information service
        /// </summary>
        /// <returns></returns>
        IServerInformationService GetServerInfo();

    }
}
