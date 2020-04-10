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
