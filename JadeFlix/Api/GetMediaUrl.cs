using System.Net;
using SimpleWebApiServer;
using System;
using Common;
using JadeFlix.Domain;
using System.Diagnostics;
using JadeFlix.Domain.ApiParameters;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class GetMediaUrl : ApiGetRequestResponse<GetMediaApiParameters>
    {
        public GetMediaUrl(HttpListenerRequestCache cache = null) : base("/api/getmediaurl/{scraper}/{media_uid}", cache) { }

        protected override GetMediaApiParameters ParseParameters(RequestParameters parameters)
        {
            return new GetMediaApiParameters()
            {
                ScraperId = parameters.GetUrlParameter("scraper"),
                Url = parameters.GetUrlParameter("media_uid").DecodeFromBase64()
            };
        }

        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, GetMediaApiParameters parameters)
        {
            if (!parameters.AreValid)
            {
                return string.Empty;
            }
            var scraper = AppContext.MediaScrapers.Get(parameters.ScraperId);
            var downloadUrl = await scraper.GetMediaDownloadUrlAsync(new Uri(parameters.Url));
            if (string.IsNullOrEmpty(downloadUrl))
            {
                return string.Empty;
            }
            Trace.WriteLine("Media Url: " + downloadUrl);
            return ToJson(new NamedUri("Download", new Uri(downloadUrl)));
        }
    }
}
