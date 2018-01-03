using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;

namespace JadeFlix.Api
{
    public class FindItem : ApiGetRequestResponse
    {
        public FindItem(HttpListenerRequestCache cache = null) : base("api/findItem/{scraper}/{name}",cache) { }
        public override bool IsCacheable => false;
        public override string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var scraperId = parameters.UrlParameters["scraper"];
            var name = parameters.UrlParameters["name"];
            
            var scraper = AppContext.MediaScrapers.Get(scraperId);

            var entries = scraper.FindTvShow(name);
            var retVal = JsonConvert.SerializeObject(entries);
            return retVal;
        }
    }
}
