using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    internal class RequestBody
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public List<RequestParameter> Parameters { get; set; }
    }
}
