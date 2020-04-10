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

using MobileAppServer.ManagedServices;
using MobileAppServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.SecurityManagement
{
    internal class SecurityManagementServiceImpl : ISecurityManagementService
    {
        private IControllerManager controllerManager;
        private IInterceptorManagerService interceptorManager;
        private ICoreServerService coreServer;
        public SecurityManagementServiceImpl()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            controllerManager = manager.GetService<IControllerManager>();
            interceptorManager = manager.GetService<IInterceptorManagerService>();
            coreServer = ServiceManager.GetInstance().GetService<ICoreServerService>("realserver");
        }

        private bool securityEnabled = false;
        private BasicSecurityDefinitions definitions;
        public void EnableSecurity(IServerUserRepository userRepository, int tokenLifetime = 3, string tokenCryptPassword = "")
        {
            if (coreServer.IsLoadBalanceEnabled())
                throw new InvalidOperationException("Authentication features cannot be enabled on a load balancing server");

            if (securityEnabled)
                throw new Exception("Aready enabled.");

            controllerManager.RegisterController(typeof(AuthorizationController));
            interceptorManager.AddInterceptor(new SecurityTokenInterceptor());
            interceptorManager.AddInterceptor(new UserRoleValidationInterceptor());

            definitions = new BasicSecurityDefinitions(userRepository,
                tokenLifetime,
                tokenCryptPassword);

            securityEnabled = true;
        }

        public BasicSecurityDefinitions GetDefinitions()
        {
            return new BasicSecurityDefinitions(definitions.Repository,
                definitions.TokenLifeTime, "");
        }

        public IReadOnlyCollection<LoggedUserInfo> GetLoggedUsers()
        {
            List<LoggedUserInfo> result = new List<LoggedUserInfo>();
            TokenRepository.Instance.Tokens.ForEach(token =>
            result.Add(new LoggedUserInfo(
                token.SessionId,
                token.User.Identifier,
                token.User.Name,
                token.CreatedAt,
                token.ExpireAt))
            );
            return result.AsReadOnly();
        }

        public IReadOnlyCollection<UserActivity> GetUserActivities(LoggedUserInfo loggedUser)
        {
            ServerToken token = TokenRepository.Instance.GetToken(loggedUser.SessionId);
            return token.GetActivities();
        }

        public ServerUser Authenticate(string userNameOrEmail, string password)
        {
            return definitions.Repository.Authenticate(userNameOrEmail, password);
        }

        public bool IsAuthenticationEnabled()
        {
            return securityEnabled;
        }
    }
}
