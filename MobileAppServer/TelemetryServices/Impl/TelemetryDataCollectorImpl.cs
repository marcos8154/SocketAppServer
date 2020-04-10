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

using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.TelemetryServices.Impl
{
    public class TelemetryDataCollectorImpl : ITelemetryDataCollector
    {
        private List<ActionError> errors;
        private List<ActionExecutionTime> actionExecutions;
        private List<DependencyInjectorExecutionTime> dependencyInjectors;
        private List<HardwareUsage> hardwareUsages;
        private List<InterceptorExecutionTime> interceptorExecutions;

        private object lckObj = new object();

        public TelemetryDataCollectorImpl()
        {
            errors = new List<ActionError>();
            actionExecutions = new List<ActionExecutionTime>();
            dependencyInjectors = new List<DependencyInjectorExecutionTime>();
            hardwareUsages = new List<HardwareUsage>();
            interceptorExecutions = new List<InterceptorExecutionTime>();
        }

        public void Collect(object eventObj)
        {
            lock (lckObj)
            {
                if (eventObj is ActionError)
                    errors.Add((ActionError)eventObj);
                if (eventObj is ActionExecutionTime)
                    actionExecutions.Add((ActionExecutionTime)eventObj);
                if (eventObj is DependencyInjectorExecutionTime)
                    dependencyInjectors.Add((DependencyInjectorExecutionTime)eventObj);
                if (eventObj is HardwareUsage)
                    hardwareUsages.Add((HardwareUsage)eventObj);
                if (eventObj is InterceptorExecutionTime)
                    interceptorExecutions.Add((InterceptorExecutionTime)eventObj);
            }
        }

        public List<ActionError> GetActionErros()
        {
            lock (lckObj)
            {
                ActionError[] result = new ActionError[errors.Count];
                errors.CopyTo(result);
                errors.Clear();
                return result.ToList();
            }
        }

        public List<ActionExecutionTime> GetActionExecutions()
        {
            lock (lckObj)
            {
                ActionExecutionTime[] result = new ActionExecutionTime[actionExecutions.Count];
                actionExecutions.CopyTo(result);
                actionExecutions.Clear();
                return result.ToList();
            }
        }

        public List<DependencyInjectorExecutionTime> GetDependencyInjectors()
        {
            lock (lckObj)
            {
                DependencyInjectorExecutionTime[] result = new DependencyInjectorExecutionTime[dependencyInjectors.Count];
                dependencyInjectors.CopyTo(result);
                dependencyInjectors.Clear();
                return result.ToList();
            }
        }

        public List<HardwareUsage> GetHardwareUsages()
        {
            lock (lckObj)
            {
                HardwareUsage[] result = new HardwareUsage[hardwareUsages.Count];
                hardwareUsages.CopyTo(result);
                hardwareUsages.Clear();
                return result.ToList();
            }
        }

        public List<InterceptorExecutionTime> GetInterceptorExecutions()
        {
            lock (lckObj)
            {
                InterceptorExecutionTime[] result = new InterceptorExecutionTime[interceptorExecutions.Count];
                interceptorExecutions.CopyTo(result);
                interceptorExecutions.Clear();
                return result.ToList();
            }
        }
    }
}
