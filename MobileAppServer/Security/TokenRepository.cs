using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;

namespace MobileAppServer.Security
{
    internal class TokenRepository
    {
        private static TokenRepository _instance;

        public static TokenRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TokenRepository();
                return _instance;
            }
        }

        public TokenRepository()
        {
            Tokens = new List<ServerToken>();
        }

        private List<ServerToken> Tokens { get; set; }

        public bool IsValid(string token)
        {
            ServerToken serverToken = Tokens.FirstOrDefault(t => t.UserToken.Equals(token));
            if (serverToken == null)
                return false;

            if (serverToken.HasExpired())
            {
                Tokens.Remove(serverToken);
                return false;
            }
            
            return true;
        }

        public ServerToken GetToken(string token)
        {
            return Tokens.FirstOrDefault(t => t.UserToken.Equals(token));
        }

        public ServerToken AddToken(ServerUser user)
        {
            ServerToken token = new ServerToken(user);
            Tokens.Add(token);
            return token;
        }
    }
}
