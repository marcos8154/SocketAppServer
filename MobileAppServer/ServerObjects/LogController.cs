using System;

namespace MobileAppServer.ServerObjects
{
    public class LogController
    {
        private static object Locked = new object();
        public static void WriteLog(ServerLog log)
        {
            Console.WriteLine(log.LogText);
            lock (Locked)
            {
                if (Server.GlobalInstance.Logger != null)
                    Server.GlobalInstance.Logger.Register(log);
            }
        }
    }
}
