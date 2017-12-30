using System.Net;
using Newtonsoft.Json;
using JadeFlix.Services;
using SimpleWebApiServer;
using System.Diagnostics;

namespace JadeFlix.Api
{
    public class GetLocal : ApiRequestResponse
    {
        public GetLocal() : base("api/getLocal/{group}/{kind}")
        {
            
        }
        public override string ProcessRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            Trace.WriteLine("Processing GetRequest");
            var kind = parameters.UrlParameters["kind"];
            if (kind == null) return string.Empty;

            var group = parameters.UrlParameters["group"];
            if (group == null) return string.Empty;

            var data = AppContext.LocalScraper.GetItems(group, kind);
            Trace.WriteLine($"Item count: {data?.Count}");
            return JsonConvert.SerializeObject(data);
        }
    }
}
