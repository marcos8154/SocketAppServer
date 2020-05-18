using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.ServerObjects
{
    /// <summary>
    /// Prevents the action from being publicly listed when requesting ServerInfoController/FullServerInfo
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NotListed : Attribute
    {
    }
}
