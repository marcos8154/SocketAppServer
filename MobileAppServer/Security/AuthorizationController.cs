using MobileAppServer.CoreServices;
using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;

namespace MobileAppServer.Security
{
    internal class AuthorizationController : IController
    {
        public ActionResult IsValidToken(string token)
        {
            bool valid = TokenRepository.Instance.IsValid(token);
            string msg = (valid
                ? "You have a valid token"
                : "You have a invalid token");
            return ActionResult.Json(new OperationResult(valid, (valid ? 600 : 500), msg), (valid ? 600 : 500),
                msg);
        }

        public ActionResult Authorize(string user, string password)
        {
            IServiceManager manager = ServiceManagerFactory.GetInstance();
            ISecurityManagementService service = manager.GetService<ISecurityManagementService>();

            var serverUser = service.Authenticate(user, password);
            if (serverUser == null)
                return ActionResult.Json(new OperationResult(string.Empty, 500, "Invalid user"), 500, "Invalid user");
            var token = TokenRepository.Instance.AddToken(serverUser);
            return ActionResult.Json(new OperationResult(token.UserToken, 600, "Authorization success. Use this token to authenticate in next requests."));
        }
    }
}
