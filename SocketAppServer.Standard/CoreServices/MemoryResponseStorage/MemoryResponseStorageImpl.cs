using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.MemoryResponseStorage
{
    internal class MemoryResponseStorageImpl : IMemoryResponseStorage
    {
        private object lck = new object();
        private List<MemoryResponseStorageItem> Items { get; set; }

        public MemoryResponseStorageImpl()
        {
            Items = new List<MemoryResponseStorageItem>();
            IServiceManager services = ServiceManager.GetInstance();
            IControllerManager manager = services.GetService<IControllerManager>();
            manager.RegisterController(typeof(ResponseStorageController));

        }

        public void CreateStorage(string id, string content)
        {
            lock (lck)
            {
                if (Items.Any(i => i.Id.Equals(id)))
                    throw new Exception("There is already a pending recovery response for the created ID.");
                Items.Add(new MemoryResponseStorageItem(id, content));
            }
        }

        public string Read(string id, int length)
        {
            lock (lck)
            {
                MemoryResponseStorageItem item = Items.FirstOrDefault(
                        i => i.Id.Equals(id));
                if (item == null)
                    return null;

                string returnContent = null;
                if (length >= item.ResponseContent.Length)
                {
                    returnContent = item.ResponseContent;
                    Items.Remove(item);
                    return returnContent;
                }

                returnContent = item.ResponseContent.Substring(0, length);
                item.SetContent(item.ResponseContent.Substring(length,
                    item.ResponseContent.Length - length));

                return returnContent;
            }
        }
    }
}