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

namespace JadeFlix.Api
{
    public class GetMediaUrl : ApiGetRequestResponse
    {
        public GetMediaUrl(HttpListenerRequestCache cache = null) : base("api/getmediaurl/{scraper}/{media_uid}",cache) { }

        public override string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var scraper = AppContext.MediaScrapers.Get(parameters.UrlParameters["scraper"]);
            if (scraper == null) return string.Empty;
            var url = parameters.UrlParameters["media_uid"].DecodeFromBase64();
            var downloadUrl = scraper.GetMediaDownloadUrl(new Uri(url));
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                Trace.WriteLine("Media Url: "+downloadUrl);
                var media = new NamedUri()
                {
                    Name = "Download",
                    Url = new Uri(downloadUrl)
                };
                return JsonConvert.SerializeObject(media);
            }
            return string.Empty;
        }
    }
}
