using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleWebApiServer
{
    public abstract class ApiRawRequestResponse : ApiRequestResponse
    {
        public ApiRawRequestResponse(string urlPattern) : base(urlPattern)
        {
        }
        public override string ProcessPostRequest(HttpListenerRequest request, RequestParameters parameters, string body)
        {
            throw new NotSupportedException();
        }
        public override string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            throw new NotSupportedException();
        }
    }
}
