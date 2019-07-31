﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServerClient
{
   public class RequestParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public RequestParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
