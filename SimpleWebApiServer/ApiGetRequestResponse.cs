using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleWebApiServer
{
    public abstract class ApiGetRequestResponse<TParams> : ApiRequestResponse<TParams>
    {
        public ApiGetRequestResponse(string urlPattern, HttpListenerRequestCache cache=null) : base(urlPattern, cache)
        {
        }
        public override string HttpMethod => "GET";
        protected override async Task<string> ProcessPostRequest(HttpListenerRequest request, TParams parameters, string postData)
        {
            await Task.Delay(10);
            throw new NotSupportedException();
        }
    }
}
