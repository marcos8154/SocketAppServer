/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
