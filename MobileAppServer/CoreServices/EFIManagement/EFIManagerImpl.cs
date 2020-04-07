using MobileAppServer.CoreServices.Logging;
using MobileAppServer.EFI;
using MobileAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.EFIManagement
{
    internal class EFIManagerImpl : IEFIManager
    {
        private List<IExtensibleFrameworkInterface> Extensions { get; set; }
        private ILoggingService logger = null;

        public EFIManagerImpl()
        {
            Extensions = new List<IExtensibleFrameworkInterface>();
            logger = ServiceManagerFactory.GetInstance().GetService<ILoggingService>();
        }

        public void AddExtension(IExtensibleFrameworkInterface extension)
        {
            Extensions.Add(extension);
        }

        public void LoadAll()
        {
            foreach (IExtensibleFrameworkInterface extension in Extensions)
            {
                try
                {
                    if (string.IsNullOrEmpty(extension.ExtensionName))
                        throw new Exception($"Cannot be load unknown extension from assembly '{extension.GetType().Assembly.FullName}'");
                    if (string.IsNullOrEmpty(extension.ExtensionVersion))
                        throw new Exception($"Cannot be read extension version for '{extension.ExtensionName}'");
                    if (string.IsNullOrEmpty(extension.ExtensionPublisher))
                        throw new Exception($"Cannot be load unknown publisher extension for '{extension.ExtensionName}'");

                    logger.WriteLog($"Loading extension '{extension.ExtensionName}', version {extension.ExtensionVersion} by {extension.ExtensionPublisher}", ServerLogType.INFO);
                    extension.Load();
                    logger.WriteLog($"Extension '{extension.ExtensionName}' successfully loaded");
                }
                catch (Exception ex)
                {
                    logger.WriteLog($"Extension '{extension.ExtensionName}' fail to load: {ex.Message}", Logging.ServerLogType.ERROR);
                }
            }
        }
    }
}
