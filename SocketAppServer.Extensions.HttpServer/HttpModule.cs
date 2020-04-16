using Microsoft.Owin.Hosting;
using MobileAppServer.EFI;
using MobileAppServer.ServerObjects;
using MobileAppServerClient;

namespace MobileAppServer.Extensions.HttpServer
{
    public class HttpModule : IExtensibleFrameworkInterface
    {
        public string ExtensionName => "HttpServerModule";
        public string ExtensionVersion => "1.1.3.0";
        public string ExtensionPublisher => "https://github.com/marcos8154";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAddress">Base address (with port) for the HttpServer</param>
        public HttpModule(string baseAddress)
        { 
            BaseAddress = baseAddress;
        }

        public string BaseAddress { get; }

        public void Load(Server server)
        {
            Client.Configure("localhost", server.Port, server.BufferSize);

            // Inicia o host OWIN 
            WebApp.Start<Startup>(url: BaseAddress);
            LogController.WriteLog($"HTTP Module was started on '{BaseAddress}'");
        }
    }
}
