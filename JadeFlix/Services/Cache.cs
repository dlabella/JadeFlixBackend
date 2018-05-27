using Common.Logging;
using JadeFlix.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JadeFlix.Services
{
    public class MemoryCache<T> where T:class
    {
        private readonly TimeSpan _cacheTime;
        private readonly Dictionary<string, CacheEntry<T>> _cache;
        private readonly int _maxEntries;
        public MemoryCache(TimeSpan cacheTime, int maxEntries=5)
        {
            _maxEntries = maxEntries;
            _cacheTime = cacheTime;
            _cache = new Dictionary<string, CacheEntry<T>>(maxEntries, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<T> GetOrAddAsync(string key, Func<Task<T>> obtainData)
        {
            if (_cache.ContainsKey(key))
            {
                var entry = _cache[key];
                if (entry.CatchUntil > DateTime.Now)
                {
                    Logger.Debug("Content served from cache");
                    return entry.Data;
                }
            }
            var data = await obtainData.Invoke();
            AddOrUpdate(key, data);

            return data;
        }


        private void AddOrUpdate(string key, T data)
        {
            if (_cache.ContainsKey(key))
            {
                Logger.Debug("Content cache updated");
                _cache[key].CatchUntil = DateTime.Now.Add(_cacheTime);
                _cache[key].Data=data;
            }
            else
            {
                if (data == default(T) || string.IsNullOrEmpty(data.ToString()))
                {
                    return;
                }

                if (_maxEntries >= _cache.Count)
                {
                    _cache.Remove(GetOldestEntry());
                }
                Logger.Debug("Content cached");
                var entry = new CacheEntry<T>()
                {
                    CatchUntil = DateTime.Now.Add(_cacheTime),
                    Data = data
                };
                _cache.Add(key, entry);
            }
        }

        private string GetOldestEntry()
        {
            CacheEntry<T> oldest=null;
            var oldestKey = string.Empty;
            foreach(var entry in _cache)
            {
                if (oldest == null)
                {
                    oldest = entry.Value;
                    oldestKey = entry.Key;
                }
                else
                {
                    if (entry.Value.CatchUntil < oldest.CatchUntil)
                    {
                        oldest = entry.Value;
                        oldestKey = entry.Key;
                    }
                }
            }
            return oldestKey;
        }
    }
}
