using System.Net;
using SimpleWebApiServer;
using System;
using Common;
using JadeFlix.Domain.ApiParameters;
using JadeFlix.Domain;

namespace JadeFlix.Api
{
    public class GetItem : ApiGetRequestResponse<GetItemApiParameters>
    {
        public GetItem(HttpListenerRequestCache cache = null) : base("api/getItem/{scraper}/{group}/{kind}/{nid}/{uid}", cache) { }

        protected override string ProcessGetRequest(HttpListenerRequest request, GetItemApiParameters apiParams)
        {
            if (!apiParams.AreValid)
            {
                return string.Empty;
            }
            Console.WriteLine("Item Name: " + apiParams.Name);

            var scraper = AppContext.MediaScrapers.Get(apiParams.ScraperId);
            var localEntry = AppContext.LocalScraper.Get(apiParams.Group, apiParams.Kind, apiParams.Name);

            if (apiParams.OnlyLocal || scraper == null)
            {
                if (localEntry == null) return string.Empty;
                return ToJson(localEntry);
            }
            else
            {
                var item = ProcessCatalogItem(apiParams, scraper, localEntry);
                return ToJson(item);
            }
        }

        private CatalogItem ProcessCatalogItem(GetItemApiParameters apiParams, MediaScraper scraper, CatalogItem local)
        {
            var entry = scraper.Get(new Uri(apiParams.Url));
            if (local != null)
            {
                entry.Watching = local.Watching;
            }
            AppContext.LocalScraper.SetLocalMedia(entry);

            SyncEntries(local, entry);

            return entry;
        }

        private void SyncEntries(CatalogItem local, CatalogItem remote)
        {
            if (AppContext.LocalScraper.Compare(local, remote) != 0)
            {
                AppContext.LocalScraper.SaveImagesToLocal(remote);
                AppContext.LocalScraper.SetLocalImages(remote);
                AppContext.LocalScraper.Save(remote);
            }
        }
        public override GetItemApiParameters ParseParameters(RequestParameters parameters)
        {
            return new GetItemApiParameters()
            {
                ScraperId = parameters.GetUrlParameter("scraper"),
                Kind = parameters.GetUrlParameter("kind"),
                Group = parameters.GetUrlParameter("group"),
                NId = parameters.GetUrlParameter("nid"),
                UId = parameters.GetUrlParameter("uid"),
                OnlyLocal = parameters.GetUrlParameter("onlyLocal") == "true"
            };
        }
    }
}
