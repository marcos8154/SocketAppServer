using Newtonsoft.Json;
using SocketAppServerClient.ClientUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SocketAppServerClient
{
    public class RequestParameter
    {
        [JsonIgnore]
        public JsonSerializerSettings SerializerSettings { get; }

        public string Name { get; set; }
        public object Value { get; set; }

        public RequestParameter(string name, object value,
            JsonSerializerSettings serializerSettings)
        {
            Name = name;
            Value = value;
            SerializerSettings = serializerSettings;

            /*
            if (value != null)
            {
                if (IsSimpleType(value.GetType()))
                    Value = $"{value}";
                else
                    FillValue(value);
            }*/
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
                    js.ApplyCustomSettings(SerializerSettings);
                    js.Serialize(jw, value);
                }
            }
            Value = sb.ToString();
        }
    }
}
