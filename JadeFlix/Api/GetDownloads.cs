using System.Net;
using JadeFlix.Domain.ApiParameters;
using SimpleWebApiServer;

namespace JadeFlix.Api
{
    public class GetDownloads : ApiGetRequestResponse<EmptyApiParameters>
    {
        public GetDownloads(HttpListenerRequestCache cache = null) : base("api/getDownloads",cache){}

        protected override string ProcessGetRequest(HttpListenerRequest request, EmptyApiParameters parameters)
        {
            var downloads = AppContext.FileDownloader.GetDownloads();

            return ToJson(downloads);
        }

        public override EmptyApiParameters ParseParameters(RequestParameters parameters)
        {
            return new EmptyApiParameters();
        }

        public override bool IsCacheable => false;
    }
}
