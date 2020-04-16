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
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using System;

namespace SocketAppServer.ScheduledServices
{
    internal class ScheduledTaskExecutorService : AsyncTask<ScheduledTask, string, bool>
    {
        private ScheduledTask Task { get; set; }

        private ILoggingService logger = null;
        public ScheduledTaskExecutorService()
        {
            logger = ServiceManager.GetInstance().GetService<ILoggingService>();
        }

        public override bool DoInBackGround(ScheduledTask task)
        {
            if (task.IsRunning)
                return false;
            Task = task;
            task.IsRunning = true;
            try
            {
           //     telemetrizar o tempo de execução das tasks
                task.RunTask();
                task.IsRunning = false;
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += $"\n{ex.InnerException.Message}";
                logger.WriteLog($"ScheduledTask execution problem: \nTaskName:{ task.TaskName} \nErrorMessage: {msg}", ServerLogType.ERROR);
                task.IsRunning = false;
                return false;
            }
        }

        public override void OnPostExecute(bool result)
        {
            if (Task == null)
                return;
            if (!Task.SaveState)
                return;
            ScheduleNextEventsRepository.Instance.SetNext(Task.TaskName,
                Task.Interval);
        }

        public override void OnPreExecute()
        {
        }

        public override void OnProgressUpdate(string progress)
        {
        }
    }
}
