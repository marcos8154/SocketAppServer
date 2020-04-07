using MobileAppServer.ScheduledServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.ScheduledTaskManagement
{
    internal class ScheduledTaskManagerImpl : IScheduledTaskManager
    {
        private List<ScheduledTask> Tasks { get; set; }

        public ScheduledTaskManagerImpl()
        {
            Tasks = new List<ScheduledTask>();
        }

        public void AddScheduledTask(ScheduledTask task)
        {
            if (string.IsNullOrEmpty(task.TaskName))
                throw new Exception("Task name is empty");
            if (task.Interval == null)
                throw new Exception("Task interval is null");
            Tasks.Add(task);
        }

        public ScheduledTask GetTaskInfo(string taskName)
        {
            return Tasks.FirstOrDefault(t => t.TaskName.Equals(taskName));
        }

        public List<string> GetTaskList()
        {
            return Tasks.Select(t => t.TaskName).ToList();
        }

        public void RunTaskNow(ScheduledTask task)
        {
            if (task == null)
                throw new Exception("Task is null.");
            ScheduledTaskExecutorService service = new ScheduledTaskExecutorService();
            service.Execute(task);
        }

        public void RunServerStartupTasks()
        {
            var startupTasks = Tasks.Where(t => t.RunOnServerStart).ToList();
            startupTasks.ForEach(t => RunTaskNow(t));
        }
    }
}
