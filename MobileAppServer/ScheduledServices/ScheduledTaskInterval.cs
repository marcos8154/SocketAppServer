using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileAppServer.ScheduledServices
{
    public class ScheduledTaskInterval
    {
        internal double Interval { get; private set; }
        internal int Days { get; }
        internal int Hours { get; }
        internal int Minutes { get; }

        public ScheduledTaskInterval(int days, int hours, int minutes)
        {
            Interval = new TimeSpan(days, hours, minutes, 0, 0).TotalMilliseconds;
            Days = days;
            Hours = hours;
            Minutes = minutes;
        }
    }
}
