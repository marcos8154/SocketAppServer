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

using SocketAppServer.CoreServices.Logging;
using SocketAppServer.EFI;
using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.EFIManagement
{
    internal class EFIManagerImpl : IEFIManager
    {
        private List<IExtensibleFrameworkInterface> Extensions { get; set; }
        private ILoggingService logger = null;

        public EFIManagerImpl()
        {
            Extensions = new List<IExtensibleFrameworkInterface>(5);
            logger = ServiceManager.GetInstance().GetService<ILoggingService>();
        }

        public void AddExtension(IExtensibleFrameworkInterface extension)
        {
            Extensions.Add(extension);
        }

        public void LoadAll()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            ICoreServerService coreServer = manager.GetService<ICoreServerService>();
            double serverVersion;
            double.TryParse(coreServer.GetServerVersion(), out serverVersion);

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

                    double minServerVersion = double.Parse(extension.MinServerVersion);
                    if (serverVersion < minServerVersion)
                        throw new Exception($"The extension '{extension.ExtensionName}' could not be loaded because it requires server v{extension.MinServerVersion}");

                    Console.ForegroundColor = ConsoleColor.Green;
                    logger.WriteLog($"      => Loading extension '{extension.ExtensionName}'");
                    logger.WriteLog($"      => version {extension.ExtensionVersion}");
                    logger.WriteLog($"      => by {extension.ExtensionPublisher}");
                    extension.Load(manager);
                    logger.WriteLog($"      => Extension '{extension.ExtensionName}' successfully loaded");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.WriteLog($"Extension '{extension.ExtensionName}' fail to load: {ex.Message}", Logging.ServerLogType.ERROR);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        public void AddExtensionFromDisk(string extensionPath)
        {
            try
            {
                IExtensibleFrameworkInterface efi = null;

                Assembly assembly = Assembly.LoadFile(extensionPath);
                Type[] types = assembly.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].GetInterface(typeof(IExtensibleFrameworkInterface).FullName) != null)
                    {
                        efi = (IExtensibleFrameworkInterface)Activator.CreateInstance(types[i]);
                        break;
                    }
                }

                AddExtension(efi);
            }
            catch (Exception ex)
            {
                throw new Exception("An implementation for 'IExtensibleFrameworkInterface' could not be found in the specified assembly.");
            }
        }
    }
}
