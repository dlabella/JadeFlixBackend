using System.Net;
using SimpleWebApiServer;
using System.Diagnostics;
using JadeFlix.Domain.ApiParameters;
using System.Threading.Tasks;

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

        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, GetRecentApiParameters parameters)
        {
            var scraper = AppContext.MediaScrapers.Get(parameters.ScraperId);
            var data = await scraper.GetRecentAsync();
            Trace.WriteLine($"Item count: {data?.Count}");
            return ToJson(data);
        }
    }
}
