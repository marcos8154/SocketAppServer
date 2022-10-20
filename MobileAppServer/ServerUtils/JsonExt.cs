using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.ServerUtils
{
    public static class JsonExt
    {
        public static void ApplyCustomSettings(this JsonSerializer serializer)
        {
            if (AppServerConfigurator.SerializerSettings == null)
                return;

            var settings = AppServerConfigurator.SerializerSettings;

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
