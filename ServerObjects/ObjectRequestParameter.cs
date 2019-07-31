using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    internal class ObjectRequestParameter
    {
        public string Alias { get; set; }
        public string TypeName { get; set; }

        public ObjectRequestParameter(string alias, string name)
        {
            Alias = alias;
            TypeName = name;
        }
    }
}
