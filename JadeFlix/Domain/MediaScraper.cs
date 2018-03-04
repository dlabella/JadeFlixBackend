using JadeFlix.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static JadeFlix.Domain.Enums;

namespace JadeFlix.Domain
{
    public abstract class MediaScraper
    {
        MemoryCache<string> _contentCache;
        protected MediaScraper(string name, EntryType kind, Uri baseUrl, TimeSpan contentCache, int maxCacheEntries = 5)
        {
            Name = name;
            BaseUrl = baseUrl;
            Kind = kind;
            _contentCache = new MemoryCache<string>(contentCache, maxCacheEntries);
        }
        protected MediaScraper(string name, EntryType kind, Uri baseUrl)
        {
            Name = name;
            BaseUrl = baseUrl;
            Kind = kind;
            _contentCache = null;
        }
        public string Name { get; internal set; }
        public Uri BaseUrl { get; internal set; }
        public EntryType Kind { get; internal set; }
        public string KindName { get { return Kind.ToString(); } }
        protected async Task<string> GetContentsAsync(Uri url)
        {
            if (_contentCache != null)
            {
                var data = _contentCache.GetOrAdd(url.ToString(), () => { return string.Empty; });
                if (data == string.Empty)
                {
                    data = await AppContext.Web.GetAsync(url);
                    _contentCache.AddOrUpdate(url.ToString(), data);
                }
                return data;
            }
            return await AppContext.Web.GetAsync(url);
        }

        public abstract Task<IEnumerable<NamedUri>> GetMediaUrlsAsync(Uri url);
        public abstract Task<CatalogItem> GetAsync(Uri url);
        public abstract Task<List<CatalogItem>> FindAsync(string name);
        public abstract Task<List<CatalogItem>> GetRecentAsync();
        public abstract Task<string> GetMediaDownloadUrlAsync(Uri url);
        public string ConcatToBaseUrl(string partialUrl)
        {
            return BaseUrl.ToString().TrimEnd('/') + "/" + partialUrl.TrimStart('/');
        }
    }
}
