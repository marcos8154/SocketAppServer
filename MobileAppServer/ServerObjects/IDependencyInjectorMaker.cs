using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public interface IDependencyInjectorMaker
    {
        string ControllerName { get; }

        object[] BuildInjectValues();
    }
}
