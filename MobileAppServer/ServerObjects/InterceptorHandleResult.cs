using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public class InterceptorHandleResult
    {
        internal bool Success { get; set; }

        internal string Message { get; set; }

        public InterceptorHandleResult(bool success,
            string message)
        {
            Success = success;
            Message = message;
        }
    }
}
