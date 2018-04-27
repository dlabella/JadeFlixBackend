using System.Net;
using SimpleWebApiServer;
using JadeFlix.Domain.ApiParameters;
using System;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class FindItem : ApiGetRequestResponse<FindItemApiParamters>
    {
        public FindItem(HttpListenerRequestCache cache = null) : base("/api/findItem/{scraper}/{name}",cache) { }
        public override bool IsCacheable => false;
        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, FindItemApiParamters apiParamters)
        {
            if (!apiParamters.AreValid)
            {
                return string.Empty;
            }

            var entries = await
                AppContext.MediaScrapers
                .Get(apiParamters.ScraperId)
                .FindAsync(apiParamters.Name);

            return ToJson(entries);
        }

        public override FindItemApiParamters ParseParameters(RequestParameters parameters)
        {
            return new FindItemApiParamters()
            {
                Name = Uri.UnescapeDataString(parameters.GetUrlParameter("name")),
                ScraperId = parameters.GetUrlParameter("scraper")
            };
        }
    }
}
