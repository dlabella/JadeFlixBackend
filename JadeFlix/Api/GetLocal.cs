using System.Net;
using SimpleWebApiServer;
using System.Diagnostics;
using JadeFlix.Domain.ApiParameters;

namespace JadeFlix.Api
{
    public class GetLocal : ApiGetRequestResponse<GetLocalApiParameters>
    {
        public GetLocal(HttpListenerRequestCache cache = null) : base("api/getLocal/{group}/{kind}",cache)
        {
            
        }
        public override bool IsCacheable => false;

        public override GetLocalApiParameters ParseParameters(RequestParameters parameters)
        {
            return new GetLocalApiParameters()
            {
                Group = parameters.UrlParameters["group"],
                Kind = parameters.UrlParameters["kind"]
            };
        }

        protected override string ProcessGetRequest(HttpListenerRequest request, GetLocalApiParameters parameters)
        {
            if (!parameters.AreValid)
            {
                return string.Empty;
            }

            var data = AppContext.LocalScraper.GetItems(parameters.Group, parameters.Kind);
            Trace.WriteLine($"Item count: {data?.Count}");
            return ToJson(data);
        }
    }
}
