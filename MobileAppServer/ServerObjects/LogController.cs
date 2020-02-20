using System;

namespace MobileAppServer.ServerObjects
{
    public class LogController
    {
        private static object Locked = new object();

        /// <summary>
        /// Obsolete for external use
        /// Dedicated for only internal use
        /// </summary>
        /// <param name="log"></param>
        [Obsolete]
        public static void WriteLog(ServerLog log)
        {
            if (log.Type == ServerLogType.INFO)
                Console.ForegroundColor = ConsoleColor.Green;
            if (log.Type == ServerLogType.ALERT)
                Console.ForegroundColor = ConsoleColor.Yellow;
            if (log.Type == ServerLogType.ERROR)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(log.LogText);
            Console.ForegroundColor = ConsoleColor.DarkGray;

            lock (Locked)
            {
                if (Server.GlobalInstance != null)
                    if (Server.GlobalInstance.Logger != null)
                        Server.GlobalInstance.Logger.Register(log);
            }
        }

        public static void WriteLog(string logText, ServerLogType type = ServerLogType.INFO)
        {
            WriteLog(new ServerLog(logText, type));
        }

        public static void WriteLog(string logText, string controller, string action, ServerLogType type = ServerLogType.INFO)
        {
            WriteLog(new ServerLog(logText, controller, action, type));
        }
    }
}
