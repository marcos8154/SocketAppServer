using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    public interface IEncodingConverterService
    {
        string ConvertToString(byte[] bytes);

        byte[] ConvertToByteArray(string str);
    }
}
