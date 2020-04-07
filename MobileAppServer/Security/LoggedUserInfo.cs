using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.Security
{
    public class LoggedUserInfo
    {
        public Guid SessionId { get; private set; }
        public string UserIdentifier { get; private set; }
        public string UserName { get; private set; }
        public DateTime LoggedAt { get; private set; }
        public DateTime ExpireAt { get; private set; }

        public LoggedUserInfo(
            Guid sessionId,
            string userIdentifier, 
            string userName, DateTime loggedAt, 
            DateTime expireAt)
        {
            SessionId = sessionId;
            UserIdentifier = userIdentifier;
            UserName = userName;
            LoggedAt = loggedAt;
            ExpireAt = expireAt;
        }
    }
}
