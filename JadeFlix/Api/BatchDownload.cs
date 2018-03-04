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
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class BatchDownload : ApiGetRequestResponse<BatchApiParams>
    {
        public static ConcurrentDictionary<string, string> _itemsProcessing = new ConcurrentDictionary<string, string>();
        public BatchDownload(HttpListenerRequestCache cache = null) : base("api/batchDownload", cache) { }
        public override bool IsCacheable => false;

        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, BatchApiParams apiParams)
        {
            if (!apiParams.AreValid) return string.Empty;

            var tvshow = await apiParams.Scraper.GetAsync(new Uri(apiParams.Url));

            foreach (var item in tvshow.Media.Remote.OrderBy(x => x.Name.Length).ThenBy(y => y.Name))
            {
                if (!await CanEnqueueDownload(tvshow, item, apiParams))
                {
                    continue;
                }
                if (!await EnqueueDownload(apiParams, tvshow, item))
                {
                    Thread.Sleep(3500);
                }
            }

            return ToJson(new { result = "OK", key = apiParams.Key.EncodeToBase64() });
        }

        private async Task<bool> CanEnqueueDownload(CatalogItem item, DownloadableNamedUri media, BatchApiParams parameters)
        {
            var local = await AppContext.LocalScraper.GetAsync(item.GroupName, item.KindName, item.Name);
            if (local != null && local.Media.Local.Any(x => string.Compare(x.Name, media.GetFileName(), true) == 0))
            {
                Logger.Debug("Skipping " + item.Name + " already in local");
                return false;
            }
            return true;
        }

        private static async Task<bool> EnqueueDownload(BatchApiParams apiParams, CatalogItem tvshow, DownloadableNamedUri item)
        {
            Logger.Debug("Getting media urls of " + item.Name);
            var mediaUrl = (await apiParams.Scraper.GetMediaUrlsAsync(item.Url)).FirstOrDefault();
            if (mediaUrl != null)
            {
                var downloadUrl = await apiParams.Scraper.GetMediaDownloadUrlAsync(mediaUrl.Url);
                var file = tvshow.GetMediaPath(item.GetFileName());
                Logger.Debug("Enqueuing download: " + item.GetFileName());
                AppContext.FileDownloader.Enqueue(item.UId, file, new Uri(downloadUrl), Web.CookieContainer);
                return true;
            }
            return false;
        }

        //private void BeginProcess(string key)
        //{
        //    _itemsProcessing.AddOrUpdate(key, key, (o, n) => key);
        //}

        //private bool IsProcessStarted(string key)
        //{
        //    return _itemsProcessing.ContainsKey(key);
        //}

        //private void EndProcess(string key)
        //{
        //    _itemsProcessing.TryRemove(key, out string val);
        //}

        public override BatchApiParams ParseParameters(RequestParameters parameters)
        {
            return new BatchApiParams
            {
                Group = parameters.GetQueryParameter("group"),
                Kind = parameters.GetQueryParameter("kind"),
                Url = parameters.GetQueryParameter("uid").DecodeFromBase64(),
                Scraper = AppContext.MediaScrapers.Get(parameters.GetQueryParameter("scraper"))
            };
        }
    }

}
