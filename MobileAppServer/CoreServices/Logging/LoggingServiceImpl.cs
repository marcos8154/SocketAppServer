using MobileAppServer.ManagedServices;
using System;

namespace MobileAppServer.CoreServices.Logging
{
    public class LoggingServiceImpl : ILoggingService
    {
        private static object lckObj = new object();
        private ILoggerWrapper loggerWrapper = null;

        internal void WriteLogInternal(ServerLog log)
        {
            if (log.Type == ServerLogType.INFO)
                Console.ForegroundColor = ConsoleColor.Green;
            if (log.Type == ServerLogType.ALERT)
                Console.ForegroundColor = ConsoleColor.Yellow;
            if (log.Type == ServerLogType.ERROR)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(log.LogText);
            Console.ForegroundColor = ConsoleColor.DarkGray;

            if (loggerWrapper != null)
                lock (lckObj)
                    loggerWrapper.Register(log);
        }

        public void WriteLog(string logText, ServerLogType type = ServerLogType.INFO)
        {
            WriteLogInternal(new ServerLog(logText, type));
        }

        public void WriteLog(string logText, string controller, string action, ServerLogType type = ServerLogType.INFO)
        {
            WriteLogInternal(new ServerLog(logText, controller, action, type));
        }

        public void SetWrapper(ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }
    }
}
