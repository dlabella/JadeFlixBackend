using System.Net;
using SimpleWebApiServer;
using System;
using JadeFlix.Services;
using Common;
using System.Linq;
using Common.Logging;
using System.Threading;
using JadeFlix.Domain;
using JadeFlix.Domain.ApiParameters;
using JadeFlix.Extensions;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class BatchDownload : ApiGetRequestResponse<BatchApiParams>
    {
        public BatchDownload(HttpListenerRequestCache cache = null) : base("/api/batchDownload", cache) { }
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

        private static async Task<bool> CanEnqueueDownload(CatalogItem item, DownloadableNamedUri media, BatchApiParams parameters)
        {
            Logger.Debug($"Parameters are valid {parameters.AreValid}");
            var local = await AppContext.LocalScraper.GetAsync(item.GroupName, item.KindName, item.Name);
            if (local == null || local.Media.Local.All(x =>
                    string.Compare(x.Name, media.GetFileName(), StringComparison.OrdinalIgnoreCase) != 0))
            {
                return true;
            }
            Logger.Debug("Skipping " + item.Name + " already in local");
            return false;
        }

        private static async Task<bool> EnqueueDownload(BatchApiParams apiParams, CatalogItem tvshow, DownloadableNamedUri item)
        {
            Logger.Debug("Getting media urls of " + item.Name);
            var mediaUrl = (await apiParams.Scraper.GetMediaUrlsAsync(item.Url)).FirstOrDefault();
            if (mediaUrl == null)
            {
                return false;
            }
            var downloadUrl = await apiParams.Scraper.GetMediaDownloadUrlAsync(mediaUrl.Url);
            var file = tvshow.GetMediaPath(item.GetFileName());
            Logger.Debug("Enqueuing download: " + item.GetFileName());
            AppContext.FileDownloader.Enqueue(item.UId, file, new Uri(downloadUrl), Web.CookieContainer);
            return true;
        }

        protected override BatchApiParams ParseParameters(RequestParameters parameters)
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
