using System.Net;
using SimpleWebApiServer;
using System.Diagnostics;
using JadeFlix.Domain.ApiParameters;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class GetLocal : ApiGetRequestResponse<GetLocalApiParameters>
    {
        public GetLocal(HttpListenerRequestCache cache = null) : base("/api/getLocal/{group}/{kind}",cache)
        {
            
        }
        public override bool IsCacheable => false;

        protected override GetLocalApiParameters ParseParameters(RequestParameters parameters)
        {
            return new GetLocalApiParameters()
            {
                Group = parameters.GetUrlParameter("group"),
                Kind = parameters.GetUrlParameter("kind")
            };
        }

        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, GetLocalApiParameters parameters)
        {
            if (!parameters.AreValid)
            {
                return string.Empty;
            }

            var data = await AppContext.LocalScraper.GetItemsAsync(parameters.Group, parameters.Kind);
            Trace.WriteLine($"Item count: {data?.Count}");
            return ToJson(data);
        }
    }
}
