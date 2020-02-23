using MobileAppServer.ServerObjects;
using System;
using System.Security.Policy;
using System.Text;

namespace MobileAppServer.Security
{
    internal class ServerToken
    {
        public Guid Guid { get; private set; }

        public ServerUser User { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime ExpireAt { get; private set; }

        public string UserToken { get; private set; }

        public bool HasExpired()
        {
            return (ExpireAt < DateTime.Now);
        }

        private string cryptoPasswd;

        public ServerToken(ServerUser user)
        {
            User = user;
            Guid = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            ExpireAt = CreatedAt.AddMinutes(Server.GlobalInstance.TokenLifeTime);
            CreateHash();
        }

        private string CreateContentString()
        {
            string str = $"{Guid.ToString()}";
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
            string passwd = Server.GlobalInstance.TokenCryptPassword;
            if (string.IsNullOrEmpty(passwd))
            {
                if (!string.IsNullOrEmpty(cryptoPasswd))
                    return cryptoPasswd;

                string cpuId = CPUID.GetCPUIdentifier();
                string guid = Guid.ToString().Replace("-", "");

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
