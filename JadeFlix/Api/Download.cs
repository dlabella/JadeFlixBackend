using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;
using System;
using System.Web;
using JadeFlix.Services;
using Common;
using System.IO;
using Common.Logging;

namespace JadeFlix.Api
{
    public class Download : ApiGetRequestResponse
    {
        public Download(HttpListenerRequestCache cache = null) : base("api/download",cache){}
        public override bool IsCacheable => false;
        public override string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var paramId = Uri.UnescapeDataString(parameters.QueryParameters["id"]);
            var paramGroup = parameters.QueryParameters["group"];
            if (paramGroup == null) return string.Empty;
            var paramKind = parameters.QueryParameters["kind"];
            if (paramKind == null) return string.Empty;
            var paramName = Uri.UnescapeDataString(parameters.QueryParameters["name"]);
            if (paramName == null) return string.Empty;
            var paramUrl = Uri.UnescapeDataString(parameters.QueryParameters["url"]);
            if (paramUrl == null) return string.Empty;
            var paramFile = Uri.UnescapeDataString(parameters.QueryParameters["file"]);
            if (paramFile == null) return string.Empty;

            var path = Path.Combine(AppContext.Config.MediaPath, paramGroup, paramKind, paramName.ToSafeName());
            var file = Path.Combine(path, paramFile.ToSafeName());

            Logger.Debug($"Equeuing download Name {file.ToSafePath()} Url:{paramUrl}");

            AppContext.FileDownloader.Enqueue(paramId, file.ToSafePath(), new Uri(paramUrl), Web.CookieContainer);

            return "{\"status\":200}";
        }
    }
}
