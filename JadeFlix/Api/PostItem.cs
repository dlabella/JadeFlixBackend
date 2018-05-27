using System.Net;
using SimpleWebApiServer;
using System;
using JadeFlix.Domain;
using System.Linq;
using Common.Logging;
using JadeFlix.Domain.ApiParameters;
using System.Threading.Tasks;
using JadeFlix.Services.Scrapers;

namespace JadeFlix.Api
{
    public class PostItem : ApiPostRequestResponse<EmptyApiParameters>
    {
        public PostItem(HttpListenerRequestCache cache = null) : base("/api/postItem", cache) { }

        protected override async Task<string> ProcessPostRequest(HttpListenerRequest request, EmptyApiParameters parameters, string postData)
        {
            var item = FromJson<CatalogItem>(postData);
            if (item == null)
            {
                return ToJson(new {status = "error"});
            }
            await LocalScraper.SaveAsync(item);
            UpdateCacheEntries(item);

            return ToJson(new { status = "ok" });
        }

        private void UpdateCacheEntries(CatalogItem item)
        {
            var cacheEntriesWithArray = new [] { "api/getRecent", "api/getLocal" };
            foreach (var cache in cacheEntriesWithArray)
            {
                UpdateCatalogItemArray(cache, item);
            }
            var cacheEntriesSingle = new [] { "api/getItem" };
            foreach (var cache in cacheEntriesSingle)
            {
                UpdateCatalogItem(cache, item);
            }
        }
        private void UpdateCatalogItemArray(string cacheFilter, CatalogItem item)
        {
            foreach (var cache in Cache.TryGetCachedItem(cacheFilter))
            {
                if (cache.Value == null)
                {
                    continue;
                }
                var cachedItems = FromJson<CatalogItem[]>(cache.Value);
                var cachedItem = cachedItems.FirstOrDefault(x => x.UId == item.UId);
                if (cachedItem != null)
                {
                    var idx = Array.IndexOf(cachedItems, cachedItem);
                    cachedItems[idx] = item;
                }
                else
                {
                    //Ups! not found in cache, but it must be in...
                    Cache.TryRemoveCachedItem(cache);
                }
                cache.Value = ToJson(cachedItems);
                Logger.Debug("Updated CatalogItem Array from cache [" + cacheFilter + "]");
            }
        }

        private void UpdateCatalogItem(string cacheFilter, CatalogItem item)
        {
            foreach (var cache in Cache.TryGetCachedItem(cacheFilter))
            {
                if (cache.Value == null || !cache.Source.Contains(item.UId))
                {
                    continue;
                }
                var cachedItem = FromJson<CatalogItem>(cache.Value);
                if (cachedItem != null && cachedItem.UId == item.UId)
                {
                    cache.Value = ToJson(item);
                }
                    
                Logger.Debug("Updated CatalogItem from cache [" + cacheFilter + "]");
            }
        }

        protected override EmptyApiParameters ParseParameters(RequestParameters parameters)
        {
            return new EmptyApiParameters();
        }
    }
}
