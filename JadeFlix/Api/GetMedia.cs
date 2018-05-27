using SimpleWebApiServer;
using System;
using System.Net;
using System.Linq;
using Common;
using JadeFlix.Domain.ApiParameters;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class GetMedia : ApiGetRequestResponse<GetMediaApiParameters>
    {
        public GetMedia(HttpListenerRequestCache cache = null) : base("/api/getMedia/{scraper}/{episode_uid}",cache) { }

        protected override GetMediaApiParameters ParseParameters(RequestParameters parameters)
        {
            return new GetMediaApiParameters()
            {
                ScraperId = parameters.GetUrlParameter("scraper"),
                Url = parameters.GetUrlParameter("episode_uid").DecodeFromBase64()
            };
        }

        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, GetMediaApiParameters parameters)
        {
            var scraper = AppContext.MediaScrapers.Get(parameters.ScraperId);
            var urls = (await scraper.GetMediaUrlsAsync(new Uri(parameters.Url))).ToList();
            return ToJson(urls);
        }
    }
}
