using System.Net;
using SimpleWebApiServer;
using System.Diagnostics;
using JadeFlix.Domain.ApiParameters;

namespace JadeFlix.Api
{
    public class GetRecent : ApiGetRequestResponse<GetRecentApiParameters>
    {
        public GetRecent(HttpListenerRequestCache cache=null) : base("api/getRecent/{scraper}",cache)
        {
            
        }

        public override GetRecentApiParameters ParseParameters(RequestParameters parameters)
        {
            return new GetRecentApiParameters()
            {
                ScraperId = parameters.GetUrlParameter("scraper")
            };
        }

        protected override string ProcessGetRequest(HttpListenerRequest request, GetRecentApiParameters parameters)
        {
            var scraper = AppContext.MediaScrapers.Get(parameters.ScraperId);
            var data = scraper.GetRecent();
            Trace.WriteLine($"Item count: {data?.Count}");
            return ToJson(data);
        }
    }
}
