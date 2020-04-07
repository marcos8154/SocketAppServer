using MobileAppServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    public interface ISecurityManagementService
    {
        void EnableSecurity(IServerUserRepository userRepository,
                 int tokenLifetime = 3, string tokenCryptPassword = "");

        bool IsAuthenticationEnabled();

        BasicSecurityDefinitions GetDefinitions();

        IReadOnlyCollection<LoggedUserInfo> GetLoggedUsers();

        IReadOnlyCollection<UserActivity> GetUserActivities(LoggedUserInfo loggedUser);

        ServerUser Authenticate(string userNameOrEmail, string password);
    }
}
