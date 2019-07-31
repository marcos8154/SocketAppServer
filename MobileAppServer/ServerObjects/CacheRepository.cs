using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileAppServer.ServerObjects
{
    public interface ICache
    {

    }

    public class Cache<T> : ICache
    {
        public delegate void ExpireEvent(Cache<T> cache);
        public event ExpireEvent Expired;

        public string Key { get; private set; }
        public T Value { get; set; }

        private System.Timers.Timer Timer { get; set; }

        private int Elapsed = 0;
        private int Limit = 0;

        public Cache(string key, T value, int timeToLive)
        {
            Limit = timeToLive;
            Key = key;
            Value = value;
            Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.AutoReset = true; ;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
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

        public static Cache<T> Get(string key)
        {
            Cache<T> result = CacheList.FirstOrDefault(e => e.Key.Equals(key));
            return result;
        }

        public static void ExpireAll(string startKey)
        {
            List<Cache<T>> list = CacheList.Where(c => c.Key.StartsWith(startKey)).ToList();

            foreach (Cache<T> cache in list)
                Cache_Expired(cache);
        }

        public static void Set(string key, T entity, int time)
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
                Cache<T> cache = new Cache<T>(key, entity, time);
                cache.Expired += Cache_Expired;
                CacheList.Add(cache);
            }
        }

        private static void Cache_Expired(Cache<T> cache)
        {
            lock (CacheList)
            {
                Console.WriteLine($"Removendo {cache.Key}...");
                CacheList.Remove(cache);
                cache = null;
            }
        }
    }
}
