using MobileAppServer.ServerObjects;
using System;

namespace MobileAppServer.ScheduledServices
{
    internal class ScheduledTaskExecutorService : AsyncTask<ScheduledTask, string, bool>
    {
        private ScheduledTask Task { get; set; }
        public override bool DoInBackGround(ScheduledTask task)
        {
            Task = task;
            try
            {
                LogController.WriteLog(new ServerLog($"ScheduledTask execution started: \nTaskName:{ task.TaskName}", ServerLogType.INFO));
                task.RunTask();
                LogController.WriteLog(new ServerLog($"ScheduledTask execution success: \nTaskName:{ task.TaskName}", ServerLogType.INFO));
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += $"\n{ex.InnerException.Message}";
                LogController.WriteLog(new ServerLog($"ScheduledTask execution problem: \nTaskName:{ task.TaskName} \nErrorMessage: {msg}", ServerLogType.ERROR));
                return false;
            }
        }

        public override void OnPostExecute(bool result)
        {
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
