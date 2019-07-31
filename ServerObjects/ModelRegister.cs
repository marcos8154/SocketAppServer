using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    internal class ModelRegister
    {
        public string ModeName { get; set; }
        public Type ModelType { get; set; }

        public ModelRegister (string name, Type type)
        {
            ModeName = name;
            ModelType = type;
        }
    }
}
