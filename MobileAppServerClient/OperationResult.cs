using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServerClient
{

    public class OperationResult
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public object Entity { get; set; }
    }
}
