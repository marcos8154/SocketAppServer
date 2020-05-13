using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.MemoryResponseStorage
{
    public class ResponseStorageController : IController
    {
        [ServerAction]
        public string ReadContent(string storageId, int length)
        {
            IServiceManager manager = ServiceManager.GetInstance();
            IMemoryResponseStorage storage = manager.GetService<IMemoryResponseStorage>();
            return storage.Read(storageId, length);
        }
    }
}
