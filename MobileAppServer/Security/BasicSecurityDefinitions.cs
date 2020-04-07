using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.Security
{
    public class BasicSecurityDefinitions
    {
        public IServerUserRepository Repository { get; private set; }
        public int TokenLifeTime { get; private set; }
        public string TokenCryptPassword { get; private set; }

        public BasicSecurityDefinitions(IServerUserRepository repository, 
            int tokenLifetime, 
            string tokenCryptPassword)
        {
            Repository = repository;
            this.TokenLifeTime = tokenLifetime;
            this.TokenCryptPassword = tokenCryptPassword;
        }
    }
}
