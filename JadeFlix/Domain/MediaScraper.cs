using JadeFlix.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static JadeFlix.Domain.Enums;

namespace JadeFlix.Domain
{
    public abstract class MediaScraper
    {
        private readonly MemoryCache<string> _contentCache;
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
        public string Name { get; private set; }
        protected Uri BaseUrl { get; private set; }
        private EntryType Kind { get; set; }
        public string KindName => Kind.ToString();

        protected async Task<string> GetContentsAsync(Uri url)
        {
            try
            {
                if (_contentCache == null)
                {
                    return await AppContext.Web.GetAsync(url);
                }

                var data = await _contentCache.GetOrAddAsync(url.ToString(), () => AppContext.Web.GetAsync(url));
                return data;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"GetContentAsync Error: {ex.Message}");
                return string.Empty;
            }
        }

        public abstract Task<IEnumerable<NamedUri>> GetMediaUrlsAsync(Uri url);
        public abstract Task<CatalogItem> GetAsync(Uri url);
        public abstract Task<List<CatalogItem>> FindAsync(string name);
        public abstract Task<List<CatalogItem>> GetRecentAsync();
        public abstract Task<string> GetMediaDownloadUrlAsync(Uri url);

        protected string ConcatToBaseUrl(string partialUrl)
        {
            return BaseUrl.ToString().TrimEnd('/') + "/" + partialUrl.TrimStart('/');
        }
    }
}
