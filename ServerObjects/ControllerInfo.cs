using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    internal class ControllerInfo
    {
        public string ControllerName { get; set; }
        public string ControllerClass { get; set; }
        public List<string> ControllerActions { get; set; }

        public ControllerInfo()
        {
            ControllerActions = new List<string>();
        }
    }
}
