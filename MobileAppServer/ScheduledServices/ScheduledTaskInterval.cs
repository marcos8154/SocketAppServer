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
        public double Interval { get; private set; }
        public int Days { get; private set; }
        public int Hours { get; private set; }
        public int Minutes { get; private set; }
        public int Seconds { get; private set; }

        public ScheduledTaskInterval(int days, int hours, int minutes, int seconds)
        {
            Interval = new TimeSpan(days, hours, minutes, seconds, 0).TotalMilliseconds;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }

        public override string ToString()
        {
            return $"Interval: {Days} days, {Hours} hours, {Minutes} minutes and {Seconds} seconds";
        }
    }
}
