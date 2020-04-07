using MobileAppServer.ServerObjects;

namespace MobileAppServer.EFI
{
    public interface IExtensibleFrameworkInterface
    {
        string ExtensionName { get; }

        string ExtensionVersion { get; }

        string ExtensionPublisher { get; }

        void Load();
    }
}
