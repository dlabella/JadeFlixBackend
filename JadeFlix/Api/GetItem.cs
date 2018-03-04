using System.Net;
using SimpleWebApiServer;
using System;
using Common;
using JadeFlix.Domain.ApiParameters;
using JadeFlix.Domain;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class GetItem : ApiGetRequestResponse<GetItemApiParameters>
    {
        public GetItem(HttpListenerRequestCache cache = null) : base("api/getItem/{scraper}/{group}/{kind}/{nid}/{uid}", cache) { }

        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, GetItemApiParameters apiParams)
        {
            if (!apiParams.AreValid)
            {
                return string.Empty;
            }
            Console.WriteLine("Item Name: " + apiParams.Name);

            var scraper = AppContext.MediaScrapers.Get(apiParams.ScraperId);
            var localEntry = await AppContext.LocalScraper.GetAsync(apiParams.Group, apiParams.Kind, apiParams.Name);

            if (apiParams.OnlyLocal || scraper == null)
            {
                if (localEntry == null) return string.Empty;
                return ToJson(localEntry);
            }
            else
            {
                var item = await ProcessCatalogItem(apiParams, scraper, localEntry);
                return ToJson(item);
            }
        }

        private async Task<CatalogItem> ProcessCatalogItem(GetItemApiParameters apiParams, MediaScraper scraper, CatalogItem local)
        {
            var entry = await scraper.GetAsync(new Uri(apiParams.Url));
            if (local != null)
            {
                entry.Watching = local.Watching;
            }
            AppContext.LocalScraper.SetLocalMedia(entry);

            await SyncEntries(local, entry);

            return entry;
        }

        private async Task SyncEntries(CatalogItem local, CatalogItem remote)
        {
            if (AppContext.LocalScraper.Compare(local, remote) != 0)
            {
                AppContext.LocalScraper.SaveImagesToLocal(remote);
                AppContext.LocalScraper.SetLocalImages(remote);
                await AppContext.LocalScraper.SaveAsync(remote);
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
