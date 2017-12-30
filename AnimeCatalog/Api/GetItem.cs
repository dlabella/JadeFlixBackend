using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;
using System;
using System.Web;
using Common;

namespace JadeFlix.Api
{
    public class GetItem : ApiRequestResponse
    {
        //JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling. };

        public GetItem() : base("api/getItem/{scraper}/{group}/{kind}/{nid}/{uid}") { }

        public override string ProcessRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var scraperId = parameters.UrlParameters["scraper"];
            var kind = parameters.UrlParameters["kind"];
            var group = parameters.UrlParameters["group"];
            var nid = parameters.UrlParameters["nid"];
            var uid = parameters.UrlParameters["uid"];

            var name = nid.DecodeFromBase64();
            var url = uid.DecodeFromBase64();

            var scraper = AppContext.MediaScrapers.Get(scraperId);
            
            var onlyLocal = parameters.QueryParameters.ContainsKey("onlyLocal") &&
                            parameters.QueryParameters["onlyLocal"] == "true";

            string retVal = string.Empty;

            var localEntry = AppContext.LocalScraper.Get(group, kind, name);

            if (onlyLocal || scraper == null)
            {
                if (localEntry == null) return retVal;
                retVal = JsonConvert.SerializeObject(localEntry);
                return retVal;
            }
            else if (string.Compare(kind, "TvShow", true) == 0 ||
                     string.Compare(kind, "Movie", true) == 0)
            {
                Console.WriteLine("Item Name: " + name);
                var entry = scraper.GetTvShow(new Uri(url));
                AppContext.LocalScraper.SetLocalMedia(entry);
                retVal = JsonConvert.SerializeObject(entry);

                if (AppContext.LocalScraper.Compare(localEntry, entry) != 0)
                {
                    AppContext.LocalScraper.SaveImagesToLocal(entry);
                    AppContext.LocalScraper.SetLocalImages(entry);
                    AppContext.LocalScraper.Save(entry);
                }

                return retVal;
            }
            return string.Empty;
        }
    }
}
