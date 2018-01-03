using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;
using System;
using System.Web;
using JadeFlix.Services;
using Common;
using System.IO;
using JadeFlix.Domain;
using System.Diagnostics;
using System.Linq;
using Common.Logging;
using System.Threading;
using System.Collections.Concurrent;

namespace JadeFlix.Api
{
    public class BatchDownload : ApiGetRequestResponse
    {
        public static ConcurrentDictionary<string, string> _itemsProcessing = new ConcurrentDictionary<string, string>();
        public BatchDownload(HttpListenerRequestCache cache = null) : base("api/batchDownload", cache) { }
        public override bool IsCacheable => false;
        public override string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var paramGroup = parameters.QueryParameters["group"];
            if (paramGroup == null) return string.Empty;
            var paramKind = parameters.QueryParameters["kind"];
            if (paramKind == null) return string.Empty;
            var url = parameters.QueryParameters["uid"].DecodeFromBase64();

            var scraper = AppContext.MediaScrapers.Get(parameters.QueryParameters["scraper"]);
            if (scraper == null) return string.Empty;
            var key = scraper + paramGroup + paramKind + url;
            if (_itemsProcessing.ContainsKey(key)) return string.Empty;
            _itemsProcessing.AddOrUpdate(key, key, (o, n) => key);

            ThreadPool.QueueUserWorkItem(o =>
            {
                var threadKey = key;
                var tvshow = scraper.GetTvShow(new Uri(url));
                var path = Path.Combine(AppContext.Config.MediaPath, paramGroup, paramKind, tvshow.Name);

                foreach (var item in tvshow.Media.Remote.OrderBy(x=>x.Name.Length).ThenBy(y=>y.Name))
                {
                    var local = AppContext.LocalScraper.Get(tvshow.GroupName, tvshow.KindName, tvshow.Name);

                    string fileName = (item.Name + ".mp4").CleanFileName();
                    if (local != null && local.Media.Local.Any(x => string.Compare(x.Name, fileName, true) == 0))
                    {
                        Logger.Debug("Skipping " + item.Name + " already in local");
                        continue;
                    }
                    Logger.Debug("Getting media urls of " + item.Name);
                    var mediaUrl = scraper.GetMediaUrls(item.Url).FirstOrDefault();
                    if (mediaUrl != null)
                    {
                        var downloadUrl = scraper.GetMediaDownloadUrl(mediaUrl.Url);
                        Logger.Debug("Enqueuing download: " + fileName);
                        var file = Path.Combine(path, fileName);
                        AppContext.FileDownloader.Enqueue(item.UId, file, new Uri(downloadUrl), Web.CookieContainer);
                        Thread.Sleep(3500);
                    }
                }
                _itemsProcessing.TryRemove(threadKey, out string val);
            });
            return "{\"result\":\"Ok\", \"key\":\"" + key.EncodeToBase64()+"\"}";
        }
    }
}
