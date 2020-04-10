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
