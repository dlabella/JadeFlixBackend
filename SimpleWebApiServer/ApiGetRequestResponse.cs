using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleWebApiServer
{
    public abstract class ApiGetRequestResponse : ApiRequestResponse
    {
        public ApiGetRequestResponse(string urlPattern, HttpListenerRequestCache cache=null) : base(urlPattern, cache)
        {
        }
        public override string HttpMethod => "GET";
        public override string ProcessPostRequest(HttpListenerRequest request, RequestParameters parameters, string postData)
        {
            throw new NotSupportedException();
        }
    }
}
