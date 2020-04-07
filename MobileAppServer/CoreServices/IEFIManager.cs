using MobileAppServer.EFI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    public interface IEFIManager
    {
        void AddExtension(IExtensibleFrameworkInterface extension);

        void LoadAll();
    }
}
