using System.Net;
using SimpleWebApiServer;
using System;
using JadeFlix.Services;
using Common;
using System.IO;
using Common.Logging;
using JadeFlix.Domain.ApiParameters;

namespace JadeFlix.Api
{
    public class Download : ApiGetRequestResponse<DownloadApiParameters>
    {
        public Download(HttpListenerRequestCache cache = null) : base("api/download", cache) { }
        public override bool IsCacheable => false;
        protected override string ProcessGetRequest(HttpListenerRequest request, DownloadApiParameters apiParams)
        {
            if (!apiParams.AreValid)
            {
                return string.Empty;
            }

            Logger.Debug($"Equeuing download Name {apiParams.FiletPath.ToSafePath()} Url:{apiParams.Url}");

            AppContext.FileDownloader.Enqueue(apiParams.Id, apiParams.FiletPath.ToSafePath(), new Uri(apiParams.Url), Web.CookieContainer);

            return ToJson(new { status = 200 });
        }
        public override DownloadApiParameters ParseParameters(RequestParameters parameters)
        {
            return new DownloadApiParameters()
            {
                Id = Uri.UnescapeDataString(parameters.QueryParameters["id"]),
                Group = parameters.QueryParameters["group"],
                Kind = parameters.QueryParameters["kind"],
                Name = Uri.UnescapeDataString(parameters.QueryParameters["name"]),
                Url = Uri.UnescapeDataString(parameters.QueryParameters["url"]),
                File = Uri.UnescapeDataString(parameters.QueryParameters["file"])
            };
        }
    }
    
}
