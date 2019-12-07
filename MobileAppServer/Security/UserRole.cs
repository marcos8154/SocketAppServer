using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.Security
{
    internal class UserRole
    {
        public UserRole(string controller, string actionName, bool enableAccess)
        {
            Controller = controller;
            ActionName = actionName;
            EnableAccess = enableAccess;
        }

        public string Controller { get; private set; }
        public string ActionName { get; private set; }
        public bool EnableAccess { get; private set; }
    }
}
