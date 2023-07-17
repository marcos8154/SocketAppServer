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

using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.ManagedServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketAppServer.ServerObjects
{
    public class Cache<T>
    {
        public delegate void ExpireEvent(Cache<T> cache);
        public event ExpireEvent Expired;

        internal int Index { get; private set; }

        public string Key { get; private set; }
        public T Value { get; set; }
        public string[] MethodsToInvokeOnExpire { get; set; }

        public System.Timers.Timer Timer { get; set; }

        private int Elapsed = 0;
        private int Limit = 0;

        public Cache(int index, string key, T value, int timeToLive,
            bool eternal = false,
            string[] methodsToInvokeOnExpire = null)
        {
            Index = index;
            MethodsToInvokeOnExpire = methodsToInvokeOnExpire;
            Limit = timeToLive;
            Key = key;
            Value = value;

            if (!eternal)
            {
                Timer = new System.Timers.Timer();
                Timer.Interval = 1000;
                Timer.AutoReset = true; ;
                Timer.Elapsed += Timer_Elapsed;
                Timer.Start();
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Elapsed == Limit)
            {
                Expired?.Invoke(this);
                if (Timer != null)
                {
                    Timer.Stop();
                    Timer.Dispose();
                    Timer = null;
                }
                return;
            }

            Elapsed++;
        }
    }

    public class CacheRepository<T>
    {
        private static List<Cache<T>> CacheList = new List<Cache<T>>();
        private static object lck = new object();
        public static Cache<T> Get(string key)
        {
            lock (lck)
            {
                return CacheList.FirstOrDefault(c => c.Key.Equals(key));
            }
        }

        public static void ExpireAll(string startKey)
        {
            List<Cache<T>> list = CacheList.Where(c => c.Key.StartsWith(startKey)).ToList();

            foreach (Cache<T> cache in list)
                Cache_Expired(cache);
        }

        public static void Set(string key, T entity, int time,
            bool eternal = false,
            string[] methodsToInvokeOnExpire = null)
        {
            lock (lck)
            {
                if (entity == null)
                    return;

                if (Get(key) != null)
                {
                    Get(key).Value = entity;
                    return;
                }

                lock (CacheList)
                {
                    int nextIndex = CacheList.Count;
                    char indexEntry = key[0];

                    Cache<T> cache = new Cache<T>(nextIndex, key, entity, time, eternal, methodsToInvokeOnExpire);
                    cache.Expired += Cache_Expired;
                    CacheList.Add(cache);
                }
            }
        }

        private static void Cache_Expired(Cache<T> cache)
        {
            lock (CacheList)
            {
                if (cache.MethodsToInvokeOnExpire != null)
                {
                    try
                    {
                        foreach (string method in cache.MethodsToInvokeOnExpire)
                            cache.Value.GetType().GetMethod(method).Invoke(cache.Value, null);
                    }
                    catch { }
                }

                cache.Expired -= Cache_Expired;
                CacheList.Remove(cache);

                cache = null;
            }
        }
    }
}
