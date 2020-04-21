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

using System;
using System.Threading.Tasks;
using System.Timers;

namespace SocketAppServer.ScheduledServices
{
    public abstract class ScheduledTask
    {
        internal bool IsRunning { get; set; }
        public bool RunOnServerStart { get; }
        internal string TaskName { get; private set; }
        public ScheduledTaskInterval Interval { get; internal set; }
        public bool SaveState { get; }

        Timer timer = new Timer();
        public ScheduledTask(string taskName,
            bool runOnServerStart, ScheduledTaskInterval interval, bool saveState = true)
        {
            TaskName = taskName;
            Interval = interval;
            SaveState = saveState;
            RunOnServerStart = runOnServerStart;

            StartTimer(TryGetLastInterval());
        }

        public ScheduledTaskInterval TryGetLastInterval()
        {
            DateTime? nextEvent = ScheduleNextEventsRepository.Instance.GetNext(TaskName);
            ScheduledTaskInterval nextEventInterval = null;
            if (nextEvent != null)
            {
                var nextDate = (nextEvent - DateTime.Now);
                nextEventInterval = new ScheduledTaskInterval((int)nextDate.Value.Days,
                  (int)nextDate.Value.TotalHours, (int)nextDate.Value.TotalMinutes,
                  (int)nextDate.Value.TotalSeconds);

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
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            StartTimer(Interval);
        }

        public override string ToString()
        {
            return TaskName;
        }

        public abstract void RunTask();
    }
}
