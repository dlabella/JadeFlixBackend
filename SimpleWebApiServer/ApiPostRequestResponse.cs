using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleWebApiServer
{
    public abstract class ApiPostRequestResponse<TParams> : ApiRequestResponse<TParams>
    {
        public override string HttpMethod => "POST";
        public override bool IsCacheable => false;
        public ApiPostRequestResponse(string urlPattern, HttpListenerRequestCache cache = null) : base(urlPattern, cache)
        {
        }
        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, TParams parameters)
        {
            await Task.Delay(10);
            throw new NotSupportedException();
        }
    }
}
