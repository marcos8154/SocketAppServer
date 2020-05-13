using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.CoreServices
{
    public interface IMemoryResponseStorage
    {
        void CreateStorage(string id, string content);

        string Read(string id, int length);
    }
}
