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

using SocketAppServer.CoreServices;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;

namespace SocketAppServer.Security
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
            IServiceManager manager = ServiceManager.GetInstance();
            ISecurityManagementService service = manager.GetService<ISecurityManagementService>();

            var serverUser = service.Authenticate(user, password);
            if (serverUser == null)
                return ActionResult.Json(new OperationResult(string.Empty, 500, "Invalid user"), 500, "Invalid user");
            var token = TokenRepository.Instance.AddToken(serverUser);
            return ActionResult.Json(new OperationResult(token.UserToken, 600, "Authorization success. Use this token to authenticate in next requests."));
        }
    }
}
