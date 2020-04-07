using MobileAppServer.ScheduledServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices
{
    public interface IScheduledTaskManager
    {
        void AddScheduledTask(ScheduledTask task);

        ScheduledTask GetTaskInfo(string taskName);

        void RunTaskNow(ScheduledTask task);

        void RunServerStartupTasks();

        List<string> GetTaskList();
    }
}
