using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MobileAppServerClient
{
    public class RequestParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public RequestParameter(string name, object value)
        {
            Name = name;

            if (IsSimpleType(value.GetType()))
                Value = $"{value}";
            else
                FillValue(value);
        }

        internal bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
              || type.Equals(typeof(string)) ||
              type.Equals(typeof(System.Decimal)) ||
              type.Equals(typeof(System.Int32)) ||
              type.Equals(typeof(System.String)) ||
              type.Equals(typeof(System.Int16)) ||
              type.Equals(typeof(System.Double)) ||
              type.Equals(typeof(System.Guid)) ||
              type.Equals(typeof(System.DateTime)) ||
              type.Equals(typeof(System.Byte));
        }

        private void FillValue(object value)
        {
            StringBuilder sb = new StringBuilder(1000);
            using (StringWriter sw = new StringWriter(sb))
            {
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    JsonSerializer js = new JsonSerializer();
                    js.Serialize(jw, value);
                }
            }
            Value = sb.ToString();
        }
    }
}
