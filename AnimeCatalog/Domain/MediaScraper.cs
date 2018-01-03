using JadeFlix.Services;
using System;
using System.Collections.Generic;
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
        protected string GetContents(Uri url)
        {
            if (_contentCache != null)
            {
                return _contentCache.GetOrAdd(url.ToString(), () => AppContext.Web.Get(url));
            }
            return AppContext.Web.Get(url);
        }
        //protected string PostJson(Uri url, string content)
        //{
        //    if (_contentCache != null)
        //    {
        //        return _contentCache.GetOrAdd(url.ToString(), () => AppContext.Web.PostJson(url, content));
        //    }
        //    return AppContext.Web.PostJson(url,content);
        //}
        public abstract IEnumerable<NamedUri> GetMediaUrls(Uri url);
        public abstract CatalogItem GetTvShow(Uri url);
        public abstract List<CatalogItem> FindTvShow(string name);
        public abstract CatalogItem GetMovie(Uri uri);
        public abstract List<CatalogItem> GetRecent();
        public abstract string GetMediaDownloadUrl(Uri url);
        public string ConcatToBaseUrl(string partialUrl)
        {
            return BaseUrl.ToString().TrimEnd('/') + "/" + partialUrl.TrimStart('/');
        }
    }
}
