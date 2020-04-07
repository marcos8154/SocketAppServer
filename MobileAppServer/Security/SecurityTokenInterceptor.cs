using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.Security
{
    internal class SecurityTokenInterceptor : IHandlerInterceptor
    {
        public string ControllerName { get { return ""; } }
        public string ActionName { get { return ""; } }

        public InterceptorHandleResult PreHandle(SocketRequest socketRequest)
        {
            if (socketRequest.Controller.GetType().Name.Equals("AuthorizationController"))
                return new InterceptorHandleResult(false, true, "", "");
            
            var paramToken = socketRequest.Parameters.FirstOrDefault(p => p.Name.Equals("authorization"));
            if (paramToken == null)
                return new InterceptorHandleResult(true, false, "Unauthorized. Check 'authorization' parameter in request body", "");

            string token = paramToken.Value.ToString();
            if (!TokenRepository.Instance.IsValid(token))
                return new InterceptorHandleResult(true, false, "Invalid or expired token. Check 'authorization' parameter in request body", "");
            
            return new InterceptorHandleResult(false, true, "Authorized", "");
        }
    }
}
