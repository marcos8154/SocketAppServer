using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MobileAppServer.ScheduledServices
{
    internal class ScheduleNextEventsRepository
    {
        private string defaultDir = $@"{Directory.GetCurrentDirectory()}\ScheduledTasks\";

        private ScheduleNextEventsRepository()
        {
            if (!Directory.Exists(defaultDir))
                Directory.CreateDirectory(defaultDir);
        }

        private static ScheduleNextEventsRepository _instance;

        public static ScheduleNextEventsRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ScheduleNextEventsRepository();
                return _instance;
            }
        }

        internal DateTime? GetNext(string taskName)
        {
            var events = GetNextEvents();
            var evt = events.FirstOrDefault(e => e.TaskName.Equals(taskName));
            if(evt == null)
                return null;
            return evt.NextEvent;
        }

        private static object locker = new object();
        internal void SetNext(string taskName, ScheduledTaskInterval baseInterval)
        {
            lock (locker)
            {
                DateTime nextEvent = DateTime.Now
                .AddDays(baseInterval.Days)
                .AddHours(baseInterval.Hours)
                .AddMinutes(baseInterval.Minutes);

                var events = GetNextEvents();
                var evt = events.FirstOrDefault(e => e.TaskName.Equals(taskName));
                if (evt == null)
                    events.Add(new ScheduleNextEvent(taskName, nextEvent));
                else
                    evt.NextEvent = nextEvent;

                SaveNextEvents(events);
            }
        }

        private List<ScheduleNextEvent> GetNextEvents()
        {
            string file = $@"{defaultDir}\next_events.stn";
            if (!File.Exists(file))
                return new List<ScheduleNextEvent>();

            string json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<List<ScheduleNextEvent>>(json);
        }

        private void SaveNextEvents(List<ScheduleNextEvent> nextEvents)
        {
            string file = $@"{defaultDir}\next_events.stn";
            string json = JsonConvert.SerializeObject(nextEvents);
            File.WriteAllText(file, json);
        }
    }
}
