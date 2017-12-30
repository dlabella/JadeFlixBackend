using SimpleWebApiServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using Common;

namespace JadeFlix.Api
{
    public class GetMedia : ApiRequestResponse
    {
        public GetMedia() : base("api/getMedia/{scraper}/{episode_uid}") { }

        public override string ProcessRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var scraper = AppContext.MediaScrapers.Get(parameters.UrlParameters["scraper"]);
            if (scraper == null) return string.Empty;
            var url = parameters.UrlParameters["episode_uid"].DecodeFromBase64();
            var urls = scraper.GetMediaUrls(new Uri(url)).ToList();
            return Newtonsoft.Json.JsonConvert.SerializeObject(urls);
        }
    }
}
