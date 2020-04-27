/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using SocketAppServer.ServerObjects;
using System;
using System.Linq;

namespace SocketAppServer.Security
{
    internal class UserRoleValidationInterceptor : IHandlerInterceptor
    {
        public string ControllerName { get { return "*"; } }
        public string ActionName { get { return "*"; } }

        public InterceptorHandleResult PreHandle(SocketRequest socketRequest)
        {
            string controllerName = socketRequest.Controller.GetType().Name;
            string actionName = socketRequest.Action;
            if (controllerName.Equals("AuthorizationController"))
                return new InterceptorHandleResult(false, true, "", "");

            var paramToken = socketRequest.Parameters.FirstOrDefault(p => p.Name.Equals("authorization"));
            if (paramToken == null)
                return new InterceptorHandleResult(true, false, "Unauthorized. Check 'authorization' parameter in request body", false);

            string token = paramToken.Value.ToString();
            if (!TokenRepository.Instance.IsValid(token))
                return new InterceptorHandleResult(true, false, "Invalid or expired token. Check 'authorization' parameter in request body", "");

            InterceptorHandleResult result = null;

            var userToken = TokenRepository.Instance.GetToken(token);
            var user = userToken.User;
            if (user.IsRolesAccessControllerEnabled)
            {
                var role = user.Roles.FirstOrDefault(r => r.Controller.Equals(controllerName)
                    && (r.ActionName.Equals(actionName) || r.ActionName.Equals("*")));
                if (role == null)
                    result = new InterceptorHandleResult(false, true, "Access granted", "");

                if (role.EnableAccess)
                    result = new InterceptorHandleResult(false, true, "Access granted", "");
                else
                    result = new InterceptorHandleResult(true, false, "Access danied", "");
            }
            else
                result = new InterceptorHandleResult(false, true, "Access granted", "");

            userToken.RegisterActivity(new UserActivity(DateTime.Now,
                socketRequest.Controller.GetType().Name, socketRequest.Action,
                result.ResponseSuccess));

            return result;
        }
    }
}
