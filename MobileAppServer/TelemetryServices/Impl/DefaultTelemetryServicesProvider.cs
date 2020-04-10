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

using MobileAppServer.CoreServices;
using MobileAppServer.ManagedServices;
using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Impl
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
        private List<ActionExecutionTime> mem_actionExec = null;
        private List<DependencyInjectorExecutionTime> mem_dependencyInjectors = null;
        private List<HardwareUsage> mem_hardwareUsages = null;
        private List<InterceptorExecutionTime> mem_interceptors = null;
        #endregion

        private ICoreServerService coreServer = null;
        public DefaultTelemetryServicesProvider()
        {
            coreServer = ServiceManager.GetInstance().GetService<ICoreServerService>();
        }

        #region data write/read methods
        private string TransformToCSVLine<T>(T eventObj) where T : class
        {
            StringBuilder sb = new StringBuilder();
            foreach (PropertyInfo info in eventObj.GetType().GetProperties())
                sb.Append($"{info.GetValue(eventObj)}|");
            return sb.ToString();
        }

        private void WriteToFile<T>(IEnumerable<T> events, string fileName)
            where T : class
        {
            if (events.Count() == 0)
                return;

            if (!Directory.Exists(@".\Telemetry.Data\"))
                Directory.CreateDirectory(@".\Telemetry.Data\");
            fileName = $@".\Telemetry.Data\{fileName}.tlm";

            using (TextWriter writer = new StreamWriter(fileName, true, coreServer.GetConfiguration().ServerEncoding))
            {
                for (int i = 0; i < events.Count(); i++)
                {
                    T evt = events.ElementAt(i);
                    writer.WriteLine(TransformToCSVLine<T>(evt));
                }

                writer.Flush();
                writer.Close();
            }
        }

        private IEnumerable<string> ReadTelemetryFile(string fileName)
        {
            if (!Directory.Exists(@".\Telemetry.Data\"))
                Directory.CreateDirectory(@".\Telemetry.Data\");
            fileName = $@".\Telemetry.Data\{fileName}.tlm";

            return File.ReadLines(fileName, coreServer.GetConfiguration().ServerEncoding);
        }
        #endregion

        #region Interface StandardMethods
        public void ActionError(IEnumerable<ActionError> error)
        {
            if (mem_errors != null)
                mem_errors.AddRange(error);
            WriteToFile<ActionError>(error, "br.com.SocketAppServer.TelemetryServices.ActionErrors");
        }

        public void ActionExecutionTime(IEnumerable<ActionExecutionTime> actions)
        {
            if (mem_actionExec != null)
                mem_actionExec.AddRange(actions);
            WriteToFile<ActionExecutionTime>(actions, "br.com.SocketAppServer.TelemetryServices.ActionExecutions");
        }

        public void HWUsage(IEnumerable<HardwareUsage> usage)
        {
            if (mem_hardwareUsages != null)
                mem_hardwareUsages.AddRange(usage);
            WriteToFile<HardwareUsage>(usage, "br.com.SocketAppServer.TelemetryServices.HWUsage");
        }

        public void DependencyInjectionExecutiontime(IEnumerable<DependencyInjectorExecutionTime> dependencyInjectors)
        {
            if (mem_dependencyInjectors != null)
                mem_dependencyInjectors.AddRange(dependencyInjectors);
            WriteToFile<DependencyInjectorExecutionTime>(dependencyInjectors, "br.com.SocketAppServer.DependencyInjectorExecutionTime.ActionErrors");
        }

        public void InterceptorExecutionTime(IEnumerable<InterceptorExecutionTime> interceptors)
        {
            WriteToFile<InterceptorExecutionTime>(interceptors, "br.com.SocketAppServer.TelemetryServices.InterceptorExecutionTime");
        }

        public IEnumerable<ActionError> GetActionErros(DateTime startDate, DateTime endDate, string controllerName, string actionName)
        {
            if (mem_errors == null)
            {
                mem_errors = new List<ActionError>();
                IEnumerable<ActionError> result = new List<ActionError>();
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

        public IEnumerable<ActionExecutionTime> GetActionExecutionTimes(DateTime startDate, DateTime endDate, string controllerName, string actionName)
        {
            if (mem_actionExec == null)
            {
                mem_actionExec = new List<ActionExecutionTime>();
                IEnumerable<ActionExecutionTime> result = new List<ActionExecutionTime>();
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

        public IEnumerable<DependencyInjectorExecutionTime> GetDependencyInjectorExecutionTimes(DateTime startDate, DateTime endDate, string controllerName)
        {
            if (mem_dependencyInjectors == null)
            {
                mem_dependencyInjectors = new List<DependencyInjectorExecutionTime>();
                IEnumerable<DependencyInjectorExecutionTime> result = new List<DependencyInjectorExecutionTime>();
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

        public IEnumerable<HardwareUsage> GetHardwareUsages(DateTime startDate, DateTime endDate)
        {
            if (mem_hardwareUsages == null)
            {
                mem_hardwareUsages = new List<HardwareUsage>();
                IEnumerable<HardwareUsage> result = new List<HardwareUsage>();
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

        public IEnumerable<InterceptorExecutionTime> GetInterceptorExecutionTimes(DateTime startDate, DateTime endDate, string controllerName, string actionName)
        {
            if (mem_interceptors == null)
            {
                mem_interceptors = new List<InterceptorExecutionTime>();
                IEnumerable<InterceptorExecutionTime> result = new List<InterceptorExecutionTime>();
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
        #endregion
    }
}
