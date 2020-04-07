using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    public interface IInterceptorManagerService
    {
        void AddInterceptor(IHandlerInterceptor interceptor);

        IReadOnlyCollection<IHandlerInterceptor> GlobalServerInterceptors();

        IReadOnlyCollection<IHandlerInterceptor> ControllerInterceptors(string controllerName);

        IReadOnlyCollection<IHandlerInterceptor> ControllerActionInterceptors(string controllerName, string actionName);

        bool PreHandleInterceptors(List<IHandlerInterceptor> interceptors,
            SocketRequest request, Socket socket);
    }
}
