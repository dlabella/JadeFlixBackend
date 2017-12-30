using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;

namespace JadeFlix.Api
{
    public class FindItem : ApiRequestResponse
    {
        public FindItem() : base("api/findItem/{scraper}/{name}") { }
        public override bool IsCacheable => false;
        public override string ProcessRequest(HttpListenerRequest request, RequestParameters parameters)
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
