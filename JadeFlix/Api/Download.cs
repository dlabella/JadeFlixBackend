using System.Net;
using SimpleWebApiServer;
using System;
using JadeFlix.Services;
using Common;
using Common.Logging;
using JadeFlix.Domain.ApiParameters;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class Download : ApiGetRequestResponse<DownloadApiParameters>
    {
        public Download(HttpListenerRequestCache cache = null) : base("/api/download", cache) { }
        public override bool IsCacheable => false;
        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, DownloadApiParameters apiParams)
        {
            if (!apiParams.AreValid)
            {
                return string.Empty;
            }

            Logger.Debug($"Equeuing download Name {apiParams.FiletPath.ToSafePath()} Url:{apiParams.Url}");

            AppContext.FileDownloader.Enqueue(apiParams.Id, apiParams.FiletPath.ToSafePath(), new Uri(apiParams.Url), Web.CookieContainer);
            await Task.Delay(10);
            return ToJson(new { status = 200 });
        }
        public override DownloadApiParameters ParseParameters(RequestParameters parameters)
        {
            return new DownloadApiParameters()
            {
                Id = Uri.UnescapeDataString(parameters.GetQueryParameter("id")),
                Group = parameters.GetQueryParameter("group"),
                Kind = parameters.GetQueryParameter("kind"),
                Name = Uri.UnescapeDataString(parameters.GetQueryParameter("name")),
                Url = Uri.UnescapeDataString(parameters.GetQueryParameter("url")),
                File = Uri.UnescapeDataString(parameters.GetQueryParameter("file"))
            };
        }
    }
    
}
