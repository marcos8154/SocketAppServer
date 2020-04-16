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
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SocketAppServer.Security
{
    internal class ServerToken
    {
        public Guid SessionId { get; private set; }

        public ServerUser User { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime ExpireAt { get; private set; }

        public string UserToken { get; private set; }

        public bool HasExpired()
        {
            return (ExpireAt < DateTime.Now);
        }

        private List<UserActivity> userActivities;

        public IReadOnlyCollection<UserActivity> GetActivities()
        {
            return userActivities.AsReadOnly();
        }

        public void RegisterActivity(UserActivity activity)
        {
            userActivities.Add(activity);
        }

        private string cryptoPasswd;

        ISecurityManagementService securityManagementService = null;
        public ServerToken(ServerUser user)
        {
            IServiceManager manager = ServiceManager.GetInstance();
            securityManagementService = manager.GetService<ISecurityManagementService>();

            userActivities = new List<UserActivity>();
            User = user;
            SessionId = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            ExpireAt = CreatedAt.AddMinutes(securityManagementService.GetDefinitions().TokenLifeTime);
            CreateHash();
        }

        private string CreateContentString()
        {
            string str = $"{SessionId.ToString()}";
            if (!string.IsNullOrEmpty(User.Identifier))
                str += $"|{User.Identifier}";
            if (!string.IsNullOrEmpty(User.Name))
                str += $"|{User.Name}";
            if (!string.IsNullOrEmpty(User.Email))
                str += $"|{User.Email}";
            if (!string.IsNullOrEmpty(User.Organization))
                str += $"|{User.Organization}";
            str += $"|{CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")}";
            str += $"|{ExpireAt.ToString("yyyy-MM-dd HH:mm:ss")}";
            return str;
        }

        private void CreateHash()
        {
            string str = CreateContentString();
            UserToken = new Crypto(str, GetCryptoPassword()).Crypt();
        }

        private string GetCryptoPassword()
        {
            string passwd = securityManagementService.GetDefinitions().TokenCryptPassword;
            if (string.IsNullOrEmpty(passwd))
            {
                if (!string.IsNullOrEmpty(cryptoPasswd))
                    return cryptoPasswd;

                string guid = SessionId.ToString().Replace("-", "");

                string str = CreateContentString();
                byte[] sha256 = Encoding.ASCII.GetBytes(str);

                int lenght = sha256.Length;
                string random = new Random(lenght).Next().ToString();

                cryptoPasswd = $"{guid}{random}";
                return cryptoPasswd;
            }
            else return passwd;
        }
    }
}
