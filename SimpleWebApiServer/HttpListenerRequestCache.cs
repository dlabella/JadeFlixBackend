using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleWebApiServer
{
    public class HttpListenerRequestCache
    {
        private const int CacheSize = 10;
        private const int SecondsInCache = 300;
        private const int SecondsForClean = 10;
        private readonly List<CachedItem> _cache = new List<CachedItem>(CacheSize);
        private readonly List<string> _nonCacheableRequest = new List<string>();
        private DateTime _expectedNextCacheClean;
        private string BasePath { get;}
        public HttpListenerRequestCache(string basePath)
        {
            BasePath = basePath;
            _expectedNextCacheClean = DateTime.Now;
        }

        public void AddNonCacheableRequest(string nonCacheableRequest)
        {
            _nonCacheableRequest.Add(nonCacheableRequest.ToLower());
        }

        public async Task<string> GetRequest(HttpListenerRequest request, Func<HttpListenerRequest, Task<string>> handleRequest)
        {
            var bypassCache = (request.QueryString.Get("nocache")?.ToLower()=="true");
            if (IsNonCacheableRequest(request.Url.LocalPath) || bypassCache)
            {
                return await handleRequest(request);
            }

            var dictKey = (request.RawUrl).GetHashCode();

            var result = GetValue(dictKey);
            if (!string.IsNullOrEmpty(result))
            {
                Logger.Debug("Content served from cache");
                return result;
            }
            result = await handleRequest(request);
            SetValue(dictKey,request.Url.ToString(), result);
            return result;
        }
        private bool IsNonCacheableRequest(string requestUrl)
        {
            var request = requestUrl.ToLower().Replace(BasePath,"");
            return _nonCacheableRequest.Any(x => x.Contains(request));
        }
        public IEnumerable<CachedItem> TryGetCachedItem(string sourceFilter)
        {
            return _cache.Where(x => x.Source.Contains(sourceFilter));
        }

        public void TryRemoveCachedItem(CachedItem item)
        {
            _cache?.Remove(item);
        }

        private string GetValue(int key)
        {
            if (_cache.Count == 0)
            {
                return default(string);
            }

            CleanCacheIfNeeded();

            foreach (var item in _cache)
            {
                if (item.Id == key)
                {
                    return item.Value;
                }
            }
            return default(string);
        }
        private void CleanCacheIfNeeded()
        {
            if (_cache.Count == 0) return;
            var now = DateTime.Now;
            if (_expectedNextCacheClean > now) return;

            _expectedNextCacheClean = _expectedNextCacheClean.AddSeconds(SecondsForClean);

            var i = 0;
            do
            {
                if (_cache[i].Expiration < now)
                {
                    _cache.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            } while (i < _cache.Count);
            Logger.Debug($"Cache Size: {_cache.Count} after clean");
        }

        private void SetValue(int key,string source, string value)
        {
            if (_cache.Count >= CacheSize)
            {
                _cache.RemoveAt(0);
            }
            _cache.Add(new CachedItem(key, source, value, DateTime.Now.AddSeconds(SecondsInCache)));
        }
    }
}
