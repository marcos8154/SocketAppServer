using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServerClient
{
    public class ControllerInfo
    {
        public string ControllerName { get; private set; }
        public string ControllerClass { get; private set; }
        public List<string> ControllerActions { get; private set; }

        public ControllerInfo()
        {
        }
    }
}
