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
        private static Dictionary<char, int[]> index = new Dictionary<char, int[]>
        {
            {'A', new int[0]},
            {'B', new int[0]},
            {'C', new int[0]},
            {'D', new int[0]},
            {'E', new int[0]},
            {'F', new int[0]},
            {'G', new int[0]},
            {'H', new int[0]},
            {'I', new int[0]},
            {'J', new int[0]},
            {'K', new int[0]},
            {'L', new int[0]},
            {'M', new int[0]},
            {'N', new int[0]},
            {'O', new int[0]},
            {'P', new int[0]},
            {'Q', new int[0]},
            {'R', new int[0]},
            {'S', new int[0]},
            {'T', new int[0]},
            {'U', new int[0]},
            {'V', new int[0]},
            {'X', new int[0]},
            {'Y', new int[0]},
            {'Z', new int[0]},
            {'W', new int[0]},

            {'a', new int[0]},
            {'b', new int[0]},
            {'c', new int[0]},
            {'d', new int[0]},
            {'e', new int[0]},
            {'f', new int[0]},
            {'g', new int[0]},
            {'h', new int[0]},
            {'i', new int[0]},
            {'j', new int[0]},
            {'k', new int[0]},
            {'l', new int[0]},
            {'m', new int[0]},
            {'n', new int[0]},
            {'o', new int[0]},
            {'p', new int[0]},
            {'q', new int[0]},
            {'r', new int[0]},
            {'s', new int[0]},
            {'t', new int[0]},
            {'u', new int[0]},
            {'v', new int[0]},
            {'x', new int[0]},
            {'y', new int[0]},
            {'z', new int[0]},
            {'w', new int[0]},

            {'ç', new int[0]},
            {'ã', new int[0]},
            {'á', new int[0]},
            {'à', new int[0]},
            {'â', new int[0]},
            {'é', new int[0]},
            {'ê', new int[0]},
            {'è', new int[0]},

            {'Ç', new int[0]},
            {'Ã', new int[0]},
            {'Á', new int[0]},
            {'À', new int[0]},
            {'Â', new int[0]},
            {'É', new int[0]},
            {'Ê', new int[0]},
            {'È', new int[0]},

            {'0', new int[0]},
            {'1', new int[0]},
            {'2', new int[0]},
            {'3', new int[0]},
            {'4', new int[0]},
            {'5', new int[0]},
            {'6', new int[0]},
            {'7', new int[0]},
            {'8', new int[0]},
            {'9', new int[0]},
            {'$', new int[0]},
            {'%', new int[0]},
            {'~', new int[0]},
            {'{', new int[0]},
            {'}', new int[0]},
            {'-', new int[0]},
            {'/', new int[0]},
            {'*', new int[0]},
            {'#', new int[0]},
            {'@', new int[0]},
            {'!', new int[0]},
            {'"', new int[0]},
        };

        private static List<Cache<T>> CacheList = new List<Cache<T>>();
        private static object lck = new object();
        public static Cache<T> Get(string key)
        {
            lock (lck)
            {
                try
                {
                    return TryOptimizedGet(key);
                }
                catch
                {
                    return CacheList.FirstOrDefault(c => c.Key.Equals(key));
                }
            }
        }

        private static Cache<T> TryOptimizedGet(string key)
        {
            char chr = key[0];
            int[] indexEntries = index[chr];

            for (int i = 0; i < indexEntries.Length; i++)
                if (CacheList[indexEntries[i]].Key == key)
                    return CacheList[indexEntries[i]];

            return null;
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

                    try
                    {
                        int[] values = index[indexEntry];
                        int[] expanded = new int[values.Length + 1];
                        values.CopyTo(expanded, 0);
                        expanded[expanded.Length - 1] = nextIndex;
                        index[indexEntry] = expanded;
                    }
                    catch { }

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

                try
                {
                    char chr = cache.Key[0];
                    int[] values = index[chr];
                    int[] contracted = values.Except(new int[] { cache.Index }).ToArray();
                    index[chr] = contracted;
                }
                catch { }

                cache = null;
            }
        }
    }
}
