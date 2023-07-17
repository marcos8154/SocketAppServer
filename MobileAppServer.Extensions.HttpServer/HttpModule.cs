using Microsoft.Owin.Hosting;
using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.EFI;
using SocketAppServer.ManagedServices;

namespace SocketAppServer.Extensions.HttpServer
{
    public class HttpModule : IExtensibleFrameworkInterface
    {
        public string ExtensionName => "HttpServerModule";
        public string ExtensionVersion => "2.1.47.0";
        public string ExtensionPublisher => "https://github.com/marcos8154";


        /// <summary>
        /// Extension that enables HTTP communication in the SocketAppServer framework
        /// </summary>
        /// <param name="baseAddress">Base address (with port) for the HttpServer</param>
        public HttpModule(string baseAddress, string webApiVersion = "v1", string webApiTitleSwagger = "Web Api")
        {
            BaseAddress = baseAddress;
            Startup.API_VERSION = webApiVersion;
            Startup.API_TITLE = webApiTitleSwagger;
        }

        public string BaseAddress { get; }
        public string MinServerVersion => "2.0.46.0";

        public void Load(IServiceManager serviceManager)
        {
            ILoggingService logging = serviceManager.GetService<ILoggingService>();
            WebApp.Start<Startup>(url: BaseAddress);
            logging.WriteLog($"HTTP Module was started on '{BaseAddress}'");
        }
    }
}
