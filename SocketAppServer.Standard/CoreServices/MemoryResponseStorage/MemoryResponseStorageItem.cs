using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices.MemoryResponseStorage
{
    internal class MemoryResponseStorageItem
    {

        public string Id { get; private set; }

        public string ResponseContent { get; private set; }

        public MemoryResponseStorageItem(string id, string responseContent)
        {
            Id = id;
            ResponseContent = responseContent;
        }

        public void SetContent(string newResponseContent)
        {
            if (ResponseContent.Equals(newResponseContent))
                return;
            ResponseContent = newResponseContent;
        }
    }
}
