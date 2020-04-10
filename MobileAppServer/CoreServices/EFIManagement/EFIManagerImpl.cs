/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
            logger = ServiceManager.GetInstance().GetService<ILoggingService>();
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
