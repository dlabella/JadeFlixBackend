using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApiServer
{
    public class HttpListenerRequestCache
    {
        private const int _cacheSize = 10;
        private const int _secondsInCache = 300;
        private const int _secondsForClean = 10;
        List<CachedItem> _cache = new List<CachedItem>(_cacheSize);
        List<string> _nonCacheableRequest = new List<string>();
        DateTime _expectedNextCacheClean;
        public string BasePath { get;}
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

        public bool TryRemoveCachedItem(CachedItem item)
        {
            return _cache.Remove(item);
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

            _expectedNextCacheClean.AddSeconds(_secondsForClean);

            int i = 0;
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

        private void ResetCache()
        {
            _cache.Clear();
        }

        private void SetValue(int key,string source, string value)
        {
            if (_cache.Count >= _cacheSize)
            {
                _cache.RemoveAt(0);
            }
            _cache.Add(new CachedItem(key, source, value, DateTime.Now.AddSeconds(_secondsInCache)));
        }
    }
}
