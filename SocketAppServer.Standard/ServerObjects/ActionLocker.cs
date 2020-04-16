/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using SocketAppServer.CoreServices;
using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketAppServer.ServerObjects
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
            IServiceManager manager = ServiceManager.GetInstance();
            ICoreServerService coreServer = manager.GetService<ICoreServerService>("realserver");

            if (coreServer.GetConfiguration().IsSingleThreaded)
                throw new Exception("Action blocking not allowed for single-threaded servers");

            lockedActions.Add(new KeyValuePair<IController, string>(controller, actionName));
        }

        public static void ReleaseLock(IController controller, string actionName)
        {
            try
            {
                var lckd = lockedActions.FirstOrDefault(l => l.Key.GetType().Name.Equals(controller.GetType().Name) &&
                   l.Value.Equals(actionName));

                if (lckd.Key != null)
                    lockedActions.Remove(lckd);
            }
            catch { }
        }
    }
}
