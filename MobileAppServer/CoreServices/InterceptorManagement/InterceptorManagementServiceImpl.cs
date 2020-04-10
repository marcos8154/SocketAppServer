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

using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;
using MobileAppServer.TelemetryServices;
using MobileAppServer.TelemetryServices.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.CoreServices.InterceptorManagement
{
    internal class InterceptorManagementServiceImpl : IInterceptorManagerService
    {
        private List<IHandlerInterceptor> Interceptors { get; set; }
        private ITelemetryDataCollector telemetry;

        public InterceptorManagementServiceImpl()
        {
            Interceptors = new List<IHandlerInterceptor>();
            telemetry = ServiceManager.GetInstance().GetService<ITelemetryDataCollector>();
        }

        public void AddInterceptor(IHandlerInterceptor interceptor)
        {
            if (string.IsNullOrEmpty(interceptor.ControllerName))
                throw new Exception("It is no longer possible to use empty strings or nulls for ControllerName. If your intention is to indicate ALL, use the '*' character inside the string");
            if (string.IsNullOrEmpty(interceptor.ActionName))
                throw new Exception("It is no longer possible to use empty strings or nulls for ActionName. If your intention is to indicate ALL, use the '*' character inside the string");
            Interceptors.Add(interceptor);
        }

        public IReadOnlyCollection<IHandlerInterceptor> ControllerActionInterceptors(string controllerName, string actionName)
        {
            return Interceptors
                .Where(i => i.ControllerName.Equals(controllerName) &&
                    i.ActionName.Equals(actionName))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyCollection<IHandlerInterceptor> ControllerInterceptors(string controllerName)
        {
            return Interceptors
                       .Where(i => i.ControllerName.Equals(controllerName) &&
                         i.ActionName.Equals("*"))
                       .ToList()
                       .AsReadOnly();
        }

        public IReadOnlyCollection<IHandlerInterceptor> GlobalServerInterceptors()
        {
            return Interceptors
                   .Where(i => i.ControllerName.Equals("*") &&
                       i.ActionName.Equals("*"))
                   .ToList()
                   .AsReadOnly();
        }

        public bool PreHandleInterceptors(List<IHandlerInterceptor> interceptors,
            SocketRequest request, Socket socket)
        {
            foreach (var interceptor in interceptors)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var handleResult = interceptor.PreHandle(request);
                sw.Stop();

                telemetry.Collect(new InterceptorExecutionTime(interceptor.ControllerName,
                    interceptor.ActionName, sw.ElapsedMilliseconds));

                if (handleResult.CancelActionInvoke)
                {
                    var response = (handleResult.ResponseSuccess
                        ? ResponseStatus.SUCCESS
                        : ResponseStatus.ERROR);

                    request.ProcessResponse(ActionResult.Json(
                        new OperationResult("", response, handleResult.Message), response, handleResult.Message), socket);
                    return false;
                }
            }

            return true;
        }
    }
}
