using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleWebApiServer
{
    public abstract class ApiPostRequestResponse : ApiRequestResponse
    {
        public override string HttpMethod => "POST";
        public override bool IsCacheable => false;
        public ApiPostRequestResponse(string urlPattern, HttpListenerRequestCache cache = null) : base(urlPattern, cache)
        {
        }
        public override string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            throw new NotSupportedException();
        }
    }
}
