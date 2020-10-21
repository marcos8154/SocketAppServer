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
using SocketAppServer.ManagedServices;
using SocketAppServer.TelemetryServices.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.TelemetryServices.Impl
{
    internal class DefaultTelemetryServicesProvider : ITelemetryServicesProvider
    {
        #region mem_Lists
        /*
         * each list will be initialized 
         * from the first call of its 
         * respective method of reading sets
         * */
        private List<ActionError> mem_errors = null;
        private object lck_mem_errors = new object();

        private List<ActionExecutionTime> mem_actionExec = null;
        private object lck_mem_actionExec = new object();

        private List<DependencyInjectorExecutionTime> mem_dependencyInjectors = null;
        private object lck_mem_dependencyInjectors = new object();

        private List<HardwareUsage> mem_hardwareUsages = null;
        private object lck_mem_hardwareUsages = new object();

        private List<InterceptorExecutionTime> mem_interceptors = null;
        private object lck_mem_interceptors = new object();
        #endregion

        private ICoreServerService coreServer = null;
        private IScheduledTaskManager taskManager = null;

        public DefaultTelemetryServicesProvider()
        {
            IServiceManager serviceManager = ServiceManager.GetInstance();
            coreServer = serviceManager.GetService<ICoreServerService>();
            taskManager = serviceManager.GetService<IScheduledTaskManager>();
        }

        private void RefreshTelemetryData()
        {
            var task = taskManager.GetTaskInfo("TelemetryProcessor");
            taskManager.RunTaskSync(task);
        }

        #region data write/read methods
        private string TransformToCSVLine<T>(T eventObj) where T : struct
        {
            StringBuilder sb = new StringBuilder();
            foreach (PropertyInfo info in eventObj.GetType().GetProperties())
                sb.Append($"{info.GetValue(eventObj)}|");
            return sb.ToString();
        }

        private void WriteToFile<T>(IEnumerable<T> events, string fileName)
            where T : struct
        {
            if (events.Count() == 0)
                return;

            if (!Directory.Exists(@".\Telemetry.Data\"))
                Directory.CreateDirectory(@".\Telemetry.Data\");
            fileName = $@".\Telemetry.Data\{fileName}.tlm";

            using (TextWriter writer = new StreamWriter(fileName, true, coreServer.GetConfiguration().ServerEncoding))
            {
                foreach (T evt in events.ToList())
                    writer.WriteLine(TransformToCSVLine<T>(evt));

                writer.Flush();
                writer.Close();
            }
        }

        private IEnumerable<string> ReadTelemetryFile(string fileName)
        {
            if (!Directory.Exists(@".\Telemetry.Data\"))
                Directory.CreateDirectory(@".\Telemetry.Data\");
            fileName = $@".\Telemetry.Data\{fileName}.tlm";

            if (!File.Exists(fileName))
                return new List<string>(0);

            return File.ReadLines(fileName, coreServer.GetConfiguration().ServerEncoding);
        }
        #endregion

        #region Interface StandardMethods
        public void ActionError(IEnumerable<ActionError> errors)
        {
            lock (lck_mem_errors)
            {
                if (mem_errors != null)
                    foreach (var error in errors)
                        mem_errors.Add(error);
                WriteToFile<ActionError>(errors.ToList(), "br.com.SocketAppServer.TelemetryServices.ActionErrors");
            }
        }

        public void ActionExecutionTime(IEnumerable<ActionExecutionTime> actions)
        {
            lock (lck_mem_actionExec)
            {
                if (mem_actionExec != null)
                    foreach (var action in actions)
                        mem_actionExec.Add(action);
                WriteToFile(actions.ToList(), "br.com.SocketAppServer.TelemetryServices.ActionExecutions");
            }
        }

        public void HWUsage(IEnumerable<HardwareUsage> usages)
        {
            lock (lck_mem_hardwareUsages)
            {
                if (mem_hardwareUsages != null)
                    foreach (var usage in usages)
                        mem_hardwareUsages.Add(usage);
                WriteToFile(usages.ToList(), "br.com.SocketAppServer.TelemetryServices.HWUsage");
            }
        }

        public void DependencyInjectionExecutiontime(IEnumerable<DependencyInjectorExecutionTime> dependencyInjectors)
        {
            lock (lck_mem_dependencyInjectors)
            {
                if (mem_dependencyInjectors != null)
                    foreach (var di in dependencyInjectors)
                        mem_dependencyInjectors.Add(di);
                WriteToFile<DependencyInjectorExecutionTime>(dependencyInjectors.ToList(), "br.com.SocketAppServer.DependencyInjectorExecutionTime.ActionErrors");
            }
        }

        public void InterceptorExecutionTime(IEnumerable<InterceptorExecutionTime> interceptors)
        {
            lock (lck_mem_interceptors)
            {
                if (mem_interceptors != null)
                    foreach (var intercp in interceptors)
                        mem_interceptors.Add(intercp);
                WriteToFile<InterceptorExecutionTime>(interceptors.ToList(), "br.com.SocketAppServer.TelemetryServices.InterceptorExecutionTime");
            }
        }

        public IEnumerable<ActionError> GetActionErrors(DateTime startDate, DateTime endDate, string controllerName, string actionName)
        {
            lock (lck_mem_errors)
            {
                RefreshTelemetryData();

                if (mem_errors == null)
                {
                    mem_errors = new List<ActionError>(1000);
                    IEnumerable<string> content = ReadTelemetryFile("br.com.SocketAppServer.TelemetryServices.ActionError");
                    foreach (var line in content)
                    {
                        string[] parts = line.Split('|');
                        mem_errors.Add(new ActionError(parts[0], parts[1], parts[2]));
                    }
                }

                var query = (from evt in mem_errors
                             where
                             evt.CalledTime >= startDate &&
                             evt.CalledTime <= endDate &&
                             evt.ActionName.Equals(actionName) &&
                             evt.ControllerName.Equals(controllerName)
                             select evt);

                return query.AsEnumerable();
            }
        }

        public IEnumerable<ActionExecutionTime> GetActionExecutionTimes(DateTime startDate, DateTime endDate, string controllerName, string actionName)
        {
            lock (lck_mem_actionExec)
            {
                RefreshTelemetryData();

                if (mem_actionExec == null)
                {
                    mem_actionExec = new List<ActionExecutionTime>(1000);
                    IEnumerable<string> content = ReadTelemetryFile("br.com.SocketAppServer.TelemetryServices.ActionExecutionTime");
                    foreach (var line in content)
                    {
                        string[] parts = line.Split('|');
                        mem_actionExec.Add(new ActionExecutionTime(parts[0], parts[1], long.Parse(parts[2])));
                    }
                }

                var query = (from evt in mem_actionExec
                             where
                             evt.CollectTime >= startDate &&
                             evt.CollectTime <= endDate &&
                             evt.ActionName.Equals(actionName) &&
                             evt.ControllerName.Equals(controllerName)
                             select evt);

                return query.AsEnumerable();
            }
        }

        public IEnumerable<DependencyInjectorExecutionTime> GetDependencyInjectorExecutionTimes(DateTime startDate, DateTime endDate, string controllerName)
        {
            lock (lck_mem_dependencyInjectors)
            {
                RefreshTelemetryData();

                if (mem_dependencyInjectors == null)
                {
                    mem_dependencyInjectors = new List<DependencyInjectorExecutionTime>(1000);
                    IEnumerable<string> content = ReadTelemetryFile("br.com.SocketAppServer.TelemetryServices.DependencyInjectorExecutionTime");
                    foreach (var line in content)
                    {
                        string[] parts = line.Split('|');
                        mem_dependencyInjectors.Add(new DependencyInjectorExecutionTime(parts[0], long.Parse(parts[1])));
                    }
                }

                var query = (from evt in mem_dependencyInjectors
                             where
                             evt.CalledTime >= startDate &&
                             evt.CalledTime <= endDate &&
                             evt.ControllerName.Equals(controllerName)
                             select evt);

                return query.AsEnumerable();
            }
        }

        public IEnumerable<HardwareUsage> GetHardwareUsages(DateTime startDate, DateTime endDate)
        {
            lock (lck_mem_hardwareUsages)
            {
                RefreshTelemetryData();

                if (mem_hardwareUsages == null)
                {
                    mem_hardwareUsages = new List<HardwareUsage>(1000);
                    IEnumerable<string> content = ReadTelemetryFile("br.com.SocketAppServer.TelemetryServices.HardwareUsage");
                    foreach (var line in content)
                    {
                        string[] parts = line.Split('|');
                        mem_hardwareUsages.Add(new HardwareUsage(double.Parse(parts[0]), double.Parse(parts[1]), int.Parse(parts[2])));
                    }
                }

                var query = (from evt in mem_hardwareUsages
                             where
                             evt.CollectedTime >= startDate &&
                             evt.CollectedTime <= endDate
                             select evt);

                return query.AsEnumerable();
            }
        }

        public IEnumerable<InterceptorExecutionTime> GetInterceptorExecutionTimes(DateTime startDate, DateTime endDate, string controllerName, string actionName)
        {
            lock (lck_mem_interceptors)
            {
                RefreshTelemetryData();

                if (mem_interceptors == null)
                {
                    mem_interceptors = new List<InterceptorExecutionTime>(1000);
                    IEnumerable<string> content = ReadTelemetryFile("br.com.SocketAppServer.TelemetryServices.InterceptorExecutionTime");
                    foreach (var line in content)
                    {
                        string[] parts = line.Split('|');
                        mem_interceptors.Add(new InterceptorExecutionTime(parts[0], parts[1], long.Parse(parts[2])));
                    }
                }

                var query = (from evt in mem_interceptors
                             where
                             evt.CollectTime >= startDate &&
                             evt.CollectTime <= endDate &&
                             evt.ControllerName.Equals(controllerName) &&
                             evt.ActionName.Equals(actionName)
                             select evt);

                return query.AsEnumerable();
            }
        }

        public int RequestsSuccessCount(string controllerName, string actionName, int lastMinutes = 10)
        {
            lock (lck_mem_actionExec)
            {
                IEnumerable<ActionExecutionTime> execs = GetActionExecutionTimes(DateTime.Now.AddMinutes(-lastMinutes),
                    DateTime.Now, controllerName, actionName);
                return execs.Count();
            }
        }

        public int RequestErrorsCount(string controllerName, string actionName, int lastMinutes = 10)
        {
            lock (lck_mem_errors)
            {
                IEnumerable<ActionError> errors = GetActionErrors(DateTime.Now.AddMinutes(-lastMinutes),
                  DateTime.Now, controllerName, actionName);
                return errors.Count();
            }
        }
        #endregion
    }
}
