using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{

    internal class OperationResult
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public object Entity { get; set; }

        public OperationResult(object entity, int status, string message)
        {
            Status = status;
            Entity = entity;
            Message = message;
        }
    }
}
