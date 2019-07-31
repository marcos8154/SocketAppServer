using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileAppServer.ServerObjects
{
    public class ActionLocker
    {
        private static List<KeyValuePair<IController, string>> lockedActions = new List<KeyValuePair<IController, string>>();

        internal static bool ActionHasLock(IController controller, string actionName)
        {
            var lckd = lockedActions.FirstOrDefault(l => l.Key.GetType().Name.Equals(controller.GetType().Name) &&
              l.Value.Equals(actionName));
  
            return (lckd.Key != null);
        }

        public static void AddLock(IController controller, string actionName)
        {
            if (Server.GlobalInstance.IsSingleThreaded)
                throw new Exception("Action blocking not allowed for single-threaded servers");

            lockedActions.Add(new KeyValuePair<IController, string>(controller, actionName));
        }

        public static void ReleaseLock(IController controller, string actionName)
        {
            var lckd = lockedActions.FirstOrDefault(l => l.Key.GetType().Name.Equals(controller.GetType().Name) &&
               l.Value.Equals(actionName));

            if (lckd.Key != null)
                lockedActions.Remove(lckd);
        }
    }
}
