using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.Security
{
    public class UserActivity
    {
        public DateTime ActivityTime { get; private set; }

        public string Controller { get; private set; }

        public string Action { get; private set; }

        public bool AccessGranted { get; private set; }

        public UserActivity(DateTime activityTime,
            string controller, string action, 
            bool accessGranted)
        {
            ActivityTime = activityTime;
            Controller = controller;
            Action = action;
            AccessGranted = accessGranted;
        }
    }
}
