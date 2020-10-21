using SocketAppServer.CoreServices;
using SocketAppServer.EFI;
using SocketAppServer.ManagedServices;

namespace SocketAppServer.Extensions.ClientMaker
{
    /// <summary>
    /// This extension will generate the source files (.cs) of classes to access your server's controllers and actions.
    ///
    ///WARNING: supported methods must have the annotation / attribute[ServerAction], and also cannot return ActionResult.Instead, return void or your return object directly
    /// </summary>
    public class SocketClientLayerGenerator : IExtensibleFrameworkInterface
    {
        public string ExtensionName => "ClientMaker";
        public string ExtensionVersion => "1.0.0.0";
        public string ExtensionPublisher => "SocketAppServer";
        public string MinServerVersion => "2.0.2.0";

        public void Load(IServiceManager manager)
        {
            ICLIHostService cliHost = manager.GetService<ICLIHostService>();
            cliHost.RegisterCLICommand("client-maker", "Generate client layer classes for access each Controller/Action on this server", new GeneratorCommand());
        }
    }
}
