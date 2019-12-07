using MobileAppServer.ServerObjects;
using System;
using System.Linq;

namespace MobileAppServer.Security
{
    internal class UserRoleValidationInterceptor : IHandlerInterceptor
    {
        public string ControllerName { get { return ""; } }
        public string ActionName { get { return ""; } }

        public InterceptorHandleResult PreHandle(SocketRequest socketRequest)
        {
            string controllerName = socketRequest.Controller.GetType().Name;
            string actionName = socketRequest.Action;
            if (socketRequest.Controller.GetType().Name.Equals("AuthorizationController") ||
                socketRequest.Controller.GetType().Name.Equals("ServerInfoController"))
                return new InterceptorHandleResult(false, true, "", "");

            var paramToken = socketRequest.Parameters.FirstOrDefault(p => p.Name.Equals("authorization"));
            if (paramToken == null)
                return new InterceptorHandleResult(true, false, "Unauthorized. Check 'authorization' parameter in request body", false);

            string token = paramToken.Value.ToString();
            if (!TokenRepository.Instance.IsValid(token))
                return new InterceptorHandleResult(true, false, "Invalid or expired token. Check 'authorization' parameter in request body", "");

            var userToken = TokenRepository.Instance.GetToken(token);
            var user = userToken.User;
            if(user.IsRolesAccessControllerEnabled)
            {
                var role = user.Roles.FirstOrDefault(r => r.Controller.Equals(controllerName) && r.ActionName.Equals(actionName));
                if (role == null)
                    return new InterceptorHandleResult(false, true, "Access granted", "");

                if (role.EnableAccess)
                    return new InterceptorHandleResult(false, true, "Access granted", "");
                else
                    return new InterceptorHandleResult(true, false, "Access danied", "");
            }

            return new InterceptorHandleResult(false, true, "Access granted", "");
        }
    }
}
