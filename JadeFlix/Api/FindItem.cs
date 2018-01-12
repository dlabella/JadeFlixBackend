using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;
using JadeFlix.Domain.ApiParameters;

namespace JadeFlix.Api
{
    public class FindItem : ApiGetRequestResponse<FindItemApiParamters>
    {
        public FindItem(HttpListenerRequestCache cache = null) : base("api/findItem/{scraper}/{name}",cache) { }
        public override bool IsCacheable => false;
        protected override string ProcessGetRequest(HttpListenerRequest request, FindItemApiParamters apiParamters)
        {
            if (!apiParamters.AreValid)
            {
                return string.Empty;
            }

            var entries = 
                AppContext.MediaScrapers
                .Get(apiParamters.ScraperId)
                .Find(apiParamters.Name);

            return ToJson(entries);
        }

        public override FindItemApiParamters ParseParameters(RequestParameters parameters)
        {
            return new FindItemApiParamters()
            {
                Name = parameters.UrlParameters["name"],
                ScraperId = parameters.UrlParameters["scraper"]
            };
        }
    }
}
