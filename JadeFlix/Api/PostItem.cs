using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;
using System;
using System.Web;
using Common;
using JadeFlix.Domain;
using System.Linq;
using Common.Logging;

namespace JadeFlix.Api
{
    public class PostItem : ApiPostRequestResponse
    {
        public PostItem(HttpListenerRequestCache cache = null) : base("api/postItem", cache) { }

        
        public override string ProcessPostRequest(HttpListenerRequest request, RequestParameters parameters, string postData)
        {
            var item = JsonConvert.DeserializeObject<CatalogItem>(postData);
            if (item != null)
            {
                AppContext.LocalScraper.Save(item);
                UpdateCacheEntries(item);

                return "{\"status\":\"ok\"}";
            }
            return "{\"status\":\"error\"}";
        }

        private void UpdateCacheEntries(CatalogItem item)
        {
            var cacheEntriesWithArray = new string[] { "api/getRecent", "api/getLocal" };
            foreach (var cache in cacheEntriesWithArray)
            {
                UpdateCatalogItemArray(cache, item);
            }
            var cacheEntriesSingle = new string[] { "api/getItem" };
            foreach (var cache in cacheEntriesSingle)
            {
                UpdateCatalogItem(cache, item);
            }
        }
        private void UpdateCatalogItemArray(string cacheFilter, CatalogItem item)
        {
            foreach (var cache in base.Cache.TryGetCachedItem(cacheFilter))
            {
                if (cache.Value != null)
                {
                    var cachedItems = JsonConvert.DeserializeObject<CatalogItem[]>(cache.Value);
                    var cachedItem = cachedItems.FirstOrDefault(x => x.UId == item.UId);
                    if (cachedItem != null)
                    {
                        var idx = Array.IndexOf(cachedItems, cachedItem);
                        cachedItems[idx] = item;
                    }
                    else
                    {
                        //Ups! not found in cache, but it must be in...
                        base.Cache.TryRemoveCachedItem(cache);
                    }
                    cache.Value = JsonConvert.SerializeObject(cachedItems);
                    Logger.Debug("Updated CatalogItem Array from cache [" + cacheFilter + "]");
                }
            }
        }

        private void UpdateCatalogItem(string cacheFilter, CatalogItem item)
        {
            foreach (var cache in base.Cache.TryGetCachedItem(cacheFilter))
            {
                if (cache.Value != null && cache.Source.Contains(item.UId))
                {
                    var cachedItem = JsonConvert.DeserializeObject<CatalogItem>(cache.Value);
                    if (cachedItem != null && cachedItem.UId == item.UId)
                    {
                        cache.Value = JsonConvert.SerializeObject(item);
                    }
                    
                    Logger.Debug("Updated CatalogItem from cache [" + cacheFilter + "]");
                }
            }
        }
    }
}
