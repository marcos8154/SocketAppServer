using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.CoreServices.ControllerManagement
{
    public class ControllerRegister
    {
        public string Name { get; internal set; }
        public Type Type { get; internal set; }
    }
}
