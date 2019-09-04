using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public class RequestParameter
    {
        public string Name { get; internal set; }
        public object Value { get; internal set; }

        internal RequestParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
