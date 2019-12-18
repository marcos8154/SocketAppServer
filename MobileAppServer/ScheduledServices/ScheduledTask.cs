using System;
using System.Timers;

namespace MobileAppServer.ScheduledServices
{
    public abstract class ScheduledTask
    {
        internal bool RunOnServerStart { get; }
        internal string TaskName { get; private set; }
        internal ScheduledTaskInterval Interval { get; }

        Timer timer = new Timer();
        public ScheduledTask(string taskName,
            bool runOnServerStart, ScheduledTaskInterval interval)
        {
            TaskName = taskName;
            Interval = interval;
            RunOnServerStart = runOnServerStart;

            StartTimer(TryGetLastInterval());
        }

        private ScheduledTaskInterval TryGetLastInterval()
        {
            DateTime? nextEvent = ScheduleNextEventsRepository.Instance.GetNext(TaskName);
            ScheduledTaskInterval nextEventInterval = null;
            if (nextEvent != null)
            {
                var nextDate = (nextEvent - DateTime.Now);
                nextEventInterval = new ScheduledTaskInterval((int)nextDate.Value.Days,
                  (int)nextDate.Value.TotalHours, (int)nextDate.Value.TotalMinutes);

                if (nextEventInterval.Interval <= 0)
                    nextEvent = null;
            }

            return (nextEvent == null
                ? Interval
                : nextEventInterval);
        }

        private void StartTimer(ScheduledTaskInterval interval)
        {
            if (timer == null)
                timer = new Timer();
            timer.Interval = interval.Interval;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var service = new ScheduledTaskExecutorService();
            service.OnCompleted += Service_OnCompleted;
            service.Execute(this);
        }

        private void Service_OnCompleted(bool result)
        {
            timer.Stop();
            timer.Dispose();
            timer = null;

            StartTimer(Interval);
        }

        public override string ToString()
        {
            return TaskName;
        }

        public abstract void RunTask();
    }
}
