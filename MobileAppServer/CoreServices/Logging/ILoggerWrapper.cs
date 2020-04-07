using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MobileAppServer.CoreServices.Logging
{
    public interface ILoggerWrapper
    {
        void Register(ServerLog log);

        List<ServerLog> List(Expression<Func<ServerLog, bool>> query);
    }
}
