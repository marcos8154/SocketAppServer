using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public interface ILoggerWrapper
    {
        void Register(ServerLog log);

        List<ServerLog> List(Expression<Func<ServerLog, bool>> query);
    }
}
