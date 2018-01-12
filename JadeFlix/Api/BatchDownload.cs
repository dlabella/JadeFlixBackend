using System.Net;
using SimpleWebApiServer;
using System;
using JadeFlix.Services;
using Common;
using System.IO;
using System.Linq;
using Common.Logging;
using System.Threading;
using System.Collections.Concurrent;
using JadeFlix.Domain;
using JadeFlix.Domain.ApiParameters;
using JadeFlix.Extensions;

namespace JadeFlix.Api
{
    public class BatchDownload : ApiGetRequestResponse<BatchApiParams>
    {
        public static ConcurrentDictionary<string, string> _itemsProcessing = new ConcurrentDictionary<string, string>();
        public BatchDownload(HttpListenerRequestCache cache = null) : base("api/batchDownload", cache) { }
        public override bool IsCacheable => false;

        protected override string ProcessGetRequest(HttpListenerRequest request, BatchApiParams apiParams)
        {
            if (!apiParams.AreValid) return string.Empty;

            BeginProcess(apiParams.Key);

            ThreadPool.QueueUserWorkItem(o =>
            {
                var threadKey = apiParams.Key;
                var tvshow = apiParams.Scraper.Get(new Uri(apiParams.Url));

                foreach (var item in tvshow.Media.Remote.OrderBy(x => x.Name.Length).ThenBy(y => y.Name))
                {
                    if (!CanEnqueueDownload(tvshow, item, apiParams))
                    {
                        continue;
                    }
                    if (!EnqueueDownload(apiParams, tvshow, item))
                    {
                        Thread.Sleep(3500);
                    }
                }

                EndProcess(threadKey);
            });
            return ToJson(new { result = "OK", key = apiParams.Key.EncodeToBase64() });
        }

        private bool CanEnqueueDownload(CatalogItem item, DownloadableNamedUri media, BatchApiParams parameters)
        {
            var local = AppContext.LocalScraper.Get(item.GroupName, item.KindName, item.Name);
            if (local != null && local.Media.Local.Any(x => string.Compare(x.Name, media.GetFileName(), true) == 0))
            {
                Logger.Debug("Skipping " + item.Name + " already in local");
                return false;
            }
            return true;
        }

        private static bool EnqueueDownload(BatchApiParams apiParams, CatalogItem tvshow, DownloadableNamedUri item)
        {
            Logger.Debug("Getting media urls of " + item.Name);
            var mediaUrl = apiParams.Scraper.GetMediaUrls(item.Url).FirstOrDefault();
            if (mediaUrl != null)
            {
                var downloadUrl = apiParams.Scraper.GetMediaDownloadUrl(mediaUrl.Url);
                var file = tvshow.GetMediaPath(item.GetFileName());
                Logger.Debug("Enqueuing download: " + item.GetFileName());
                AppContext.FileDownloader.Enqueue(item.UId, file, new Uri(downloadUrl), Web.CookieContainer);
                return true;
            }
            return false;
        }

        private void BeginProcess(string key)
        {
            _itemsProcessing.AddOrUpdate(key, key, (o, n) => key);
        }

        private bool IsProcessStarted(string key)
        {
            return _itemsProcessing.ContainsKey(key);
        }

        private void EndProcess(string key)
        {
            _itemsProcessing.TryRemove(key, out string val);
        }

        public override BatchApiParams ParseParameters(RequestParameters parameters)
        {
            return new BatchApiParams
            {
                Group = parameters.QueryParameters["group"],
                Kind = parameters.QueryParameters["kind"],
                Url = parameters.QueryParameters["uid"].DecodeFromBase64(),
                Scraper = AppContext.MediaScrapers.Get(parameters.QueryParameters["scraper"])
            };
        }
    }

}
