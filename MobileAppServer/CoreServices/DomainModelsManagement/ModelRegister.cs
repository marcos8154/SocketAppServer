using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.CoreServices.DomainModelsManagement
{
    public class ModelRegister
    {
        public string ModeName { get; internal set; }
        public Type ModelType { get; internal set; }

        internal ModelRegister(string name, Type type)
        {
            ModeName = name;
            ModelType = type;
        }
    }
}
