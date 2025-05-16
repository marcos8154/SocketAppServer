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

using SocketAppServer.ManagedServices;
using System;

namespace SocketAppServer.CoreServices.Logging
{
    public class LoggingServiceImpl : ILoggingService
    {
        private static object lckObj = new object();
        private ILoggerWrapper loggerWrapper = null;
        private ICLIHostService cliHost = null;
        public LoggingServiceImpl()
        {
            cliHost = ServiceManager.GetInstance().GetService<ICLIHostService>();
        }

        internal void WriteLogInternal(ServerLog log)
        {
            if (!cliHost.IsCLIBusy())
               if (!CSL.ConsoleDisabled) Console.WriteLine($"[{log.EventDate} {log.Type}]: {log.LogText}");
            if (loggerWrapper != null)
                lock (lckObj)
                    loggerWrapper.Register(ref log);
            log = null;
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
