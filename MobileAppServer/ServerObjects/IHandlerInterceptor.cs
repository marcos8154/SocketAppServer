using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public interface IHandlerInterceptor
    {
        string ControllerName { get; }

        string ActionName { get; }

        InterceptorHandleResult PreHandle(SocketRequest socketRequest);
    }
}
