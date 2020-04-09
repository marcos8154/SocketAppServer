using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer.ManagedServices
{
    internal class ServiceManagerImpl : IServiceManager
    {
        private object lckObj = new object();
        private List<ManagedServiceBinding> Bindings { get; set; }

        public ServiceManagerImpl()
        {
            Bindings = new List<ManagedServiceBinding>();
        }

        public void Bind<T>(Type implementation, bool singletonInstance)
        {
            if (!typeof(T).IsInterface)
                throw new Exception("'T' is not a interface.");
            lock (lckObj)
                Bindings.Add(new ManagedServiceBinding(typeof(T), implementation, singletonInstance));
        }

        public void Bind<T>(string alias, Type implementation, bool singletonInstance)
        {
            if (!typeof(T).IsInterface)
                throw new Exception("'T' is not a interface.");
            lock (lckObj)
                Bindings.Add(new ManagedServiceBinding(alias, typeof(T), implementation, singletonInstance));
        }

        public void Unbind<T>()
        {
            Type interfaceType = typeof(T);
            var bindings = Bindings.Where(b => b.InterfaceType.Equals(interfaceType)).ToList();
            if (bindings == null)
                throw new Exception("Service instance not found.");

            lock (lckObj)
            {
                bindings.ForEach(b => b.DestroySingleton());
                Bindings.RemoveAll(b => b.InterfaceType.Equals(interfaceType));
            }
        }

        public void Unbind(string alias)
        {
            var bind = Bindings.FirstOrDefault(b => alias.Equals(b.Name));
            if (bind == null)
                throw new Exception("Service instance not found.");

            lock (lckObj)
            {
                bind.DestroySingleton();
                Bindings.Remove(bind);
                bind = null;
            }
        }

        public void Unbind(Type implementation)
        {
            var bind = Bindings.FirstOrDefault(b => b.Implementation.Equals(implementation));
            if (bind == null)
                throw new Exception("Service instance not found.");

            lock (lckObj)
            {
                bind.DestroySingleton();
                Bindings.Remove(bind);
                bind = null;
            }
        }

        private T GetServiceInternal<T>(ManagedServiceBinding bind, params object[] args)
        {
            if (bind.Singleton)
            {
                bind.InitializeSingleton(args);
                return (T)bind.SingletonInstance;
            }

            Object concreteInstance = Activator.CreateInstance(bind.Implementation, args);
            return (T)concreteInstance;
        }

        public T GetService<T>(string alias, params object[] args)
        {
            try
            {
                var bind = Bindings.FirstOrDefault(b => alias.Equals(b.Name));
                if (bind == null)
                    throw new Exception("Service instance not found.");

                return GetServiceInternal<T>(bind, args);
            }
            catch
            {
                return default(T);
            }
        }

        public T GetService<T>(Type implementation, params object[] args)
        {
            try
            {
                var bind = Bindings.FirstOrDefault(b => b.Implementation.Equals(implementation));
                if (bind == null)
                    throw new Exception("Service instance not found.");

                return GetServiceInternal<T>(bind, args);
            }
            catch
            {
                return default(T);
            }
        }

        public IReadOnlyCollection<ManagedServiceBinding> GetAllServices()
        {
            return Bindings.AsReadOnly();
        }

        public T GetService<T>(params object[] args)
        {
            try
            {
                var bind = Bindings.FirstOrDefault(b => b.InterfaceType.Equals(typeof(T)) &&
                    string.IsNullOrEmpty(b.Name));
                if (bind == null)
                    throw new Exception("Service instance not found.");
                return GetServiceInternal<T>(bind, args);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
