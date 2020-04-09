using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.ManagedServices
{
    public class ManagedServiceBinding
    {
        public override string ToString()
        {
            return $"{InterfaceType.Name}@{Implementation.Name}";
        }

        internal ManagedServiceBinding(Type interfaceType, Type implementation,
            bool singleton)
        {
            InterfaceType = interfaceType;
            Implementation = implementation;
            Singleton = singleton;
        }

        internal ManagedServiceBinding(string name,
            Type interfaceType,
            Type implementation, bool singleton)
        {
            Name = name;
            InterfaceType = interfaceType;
            Implementation = implementation;
            Singleton = singleton;
        }

        internal void InitializeSingleton(params object[] args)
        {
            if (SingletonInstance == null)
                SingletonInstance = Activator.CreateInstance(Implementation, args);
        }

        internal void DestroySingleton()
        {
            if (SingletonInstance == null)
                return;
            if (SingletonInstance is IDisposable)
                (SingletonInstance as IDisposable).Dispose();
            SingletonInstance = null;
        }

        internal Object SingletonInstance { get; private set; }
        public Type InterfaceType { get; private set; }
        public Type Implementation { get; private set; }
        public bool Singleton { get; private set; }
        public string Name { get; private set; }
    }
}
