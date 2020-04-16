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

namespace SocketAppServer.ManagedServices
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
