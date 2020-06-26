using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient.ClientUtils
{
    public static class JsonExt
    {
        public static void ApplyCustomSettings(this JsonSerializer serializer,
            JsonSerializerSettings settings)
        {
            if (settings == null)
                return;

            foreach (PropertyInfo prop in serializer.GetType().GetProperties())
            {
                try
                {
                    var value = settings.GetType().GetProperty(prop.Name).GetValue(settings);
                    if (value == null)
                        continue;
                    prop.SetValue(serializer, value);
                }
                catch { }
            }
        }
    }
}
