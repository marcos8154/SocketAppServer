using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServerManager2.Shared
{
    public static class ObjectCopy
    {
        public static object CopyFrom(this object target, object source)
        {
            foreach (PropertyInfo pInfo in source.GetType().GetProperties())
                target.GetType().GetProperty(pInfo.Name)
                    .SetValue(target, pInfo.GetValue(source, null), null);

            return target;
        }
    }
}
