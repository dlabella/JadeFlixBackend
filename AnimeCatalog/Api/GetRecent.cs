using System.Net;
using Newtonsoft.Json;
using JadeFlix.Services;
using SimpleWebApiServer;
using System.Diagnostics;

namespace JadeFlix.Api
{
    public class GetRecent : ApiRequestResponse
    {
        public GetRecent() : base("api/getRecent/{scraper}")
        {
            
        }
        public override string ProcessRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            Trace.WriteLine("Processing GetRequest");
            var scraper = AppContext.MediaScrapers.Get(parameters.UrlParameters["scraper"]);
            Trace.WriteLine($"Using scraper {scraper?.Name}");
            if (scraper == null) return string.Empty;
            Trace.WriteLine($"Getting recent items");
            var data = scraper.GetRecent();
            Trace.WriteLine($"Item count: {data?.Count}");
            return JsonConvert.SerializeObject(data);
        }
    }
}
