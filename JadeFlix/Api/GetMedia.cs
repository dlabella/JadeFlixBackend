using SimpleWebApiServer;
using System;
using System.Net;
using System.Linq;
using Common;
using JadeFlix.Domain.ApiParameters;

namespace JadeFlix.Api
{
    public class GetMedia : ApiGetRequestResponse<GetMediaApiParameters>
    {
        public GetMedia(HttpListenerRequestCache cache = null) : base("api/getMedia/{scraper}/{episode_uid}",cache) { }

        public override GetMediaApiParameters ParseParameters(RequestParameters parameters)
        {
            return new GetMediaApiParameters()
            {
                ScraperId = parameters.UrlParameters["scraper"],
                Url = parameters.UrlParameters["episode_uid"]?.DecodeFromBase64()
            };
        }

        protected override string ProcessGetRequest(HttpListenerRequest request, GetMediaApiParameters parameters)
        {
            var scraper = AppContext.MediaScrapers.Get(parameters.ScraperId);
            var urls = scraper.GetMediaUrls(new Uri(parameters.Url)).ToList();
            return ToJson(urls);
        }
    }
}
