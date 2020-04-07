using MobileAppServer.CoreServices;
using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace MobileAppServer.Security
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
            IServiceManager manager = ServiceManagerFactory.GetInstance();
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
            byte[] sha256 = Encoding.ASCII.GetBytes(str);
            string hash = Hash.CreateSHA256(sha256).ToString().Replace("-", "");
            UserToken = new Crypto(hash, GetCryptoPassword()).Crypt();
        }

        private string GetCryptoPassword()
        {
            string passwd = securityManagementService.GetDefinitions().TokenCryptPassword;
            if (string.IsNullOrEmpty(passwd))
            {
                if (!string.IsNullOrEmpty(cryptoPasswd))
                    return cryptoPasswd;

                string cpuId = CPUID.GetCPUIdentifier();
                string guid = SessionId.ToString().Replace("-", "");

                string str = CreateContentString();
                byte[] sha256 = Encoding.ASCII.GetBytes(str);

                int lenght = sha256.Length;
                string random = new Random(lenght).Next().ToString();

                cryptoPasswd = $"{cpuId}{guid}{random}";
                return cryptoPasswd;
            }
            else return passwd;
        }
    }
}
