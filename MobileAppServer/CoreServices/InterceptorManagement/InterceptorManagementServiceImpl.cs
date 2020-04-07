using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.InterceptorManagement
{
    internal class InterceptorManagementServiceImpl : IInterceptorManagerService
    {
        private List<IHandlerInterceptor> Interceptors { get; set; }

        public InterceptorManagementServiceImpl()
        {
            Interceptors = new List<IHandlerInterceptor>();
        }

        public void AddInterceptor(IHandlerInterceptor interceptor)
        {
            if (string.IsNullOrEmpty(interceptor.ControllerName))
                throw new Exception("It is no longer possible to use empty strings or nulls for ControllerName. If your intention is to indicate ALL, use the '*' character inside the string");
            if (string.IsNullOrEmpty(interceptor.ActionName))
                throw new Exception("It is no longer possible to use empty strings or nulls for ActionName. If your intention is to indicate ALL, use the '*' character inside the string");
            Interceptors.Add(interceptor);
        }

        public IReadOnlyCollection<IHandlerInterceptor> ControllerActionInterceptors(string controllerName, string actionName)
        {
            return Interceptors
                .Where(i => i.ControllerName.Equals(controllerName) &&
                    i.ActionName.Equals(actionName))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyCollection<IHandlerInterceptor> ControllerInterceptors(string controllerName)
        {
            return Interceptors
                       .Where(i => i.ControllerName.Equals(controllerName) &&
                         i.ActionName.Equals("*"))
                       .ToList()
                       .AsReadOnly();
        }

        public IReadOnlyCollection<IHandlerInterceptor> GlobalServerInterceptors()
        {
            return Interceptors
                   .Where(i => i.ControllerName.Equals("*") &&
                       i.ActionName.Equals("*"))
                   .ToList()
                   .AsReadOnly();
        }

        public bool PreHandleInterceptors(List<IHandlerInterceptor> interceptors,
            SocketRequest request, Socket socket)
        {
            foreach (var interceptor in interceptors)
            {
                var handleResult = interceptor.PreHandle(request);
                if (handleResult.CancelActionInvoke)
                {
                    var response = (handleResult.ResponseSuccess
                        ? ResponseStatus.SUCCESS
                        : ResponseStatus.ERROR);

                    request.ProcessResponse(ActionResult.Json(
                        new OperationResult("", response, handleResult.Message), response, handleResult.Message), socket);
                    return false;
                }
            }

            return true;
        }
    }
}
