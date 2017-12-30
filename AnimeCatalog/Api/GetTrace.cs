using SimpleWebApiServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using Common;
using JadeFlix.Services;

namespace JadeFlix.Api
{
    public class GetTrace : ApiRequestResponse
    {
        public GetTrace() : base("api/getTrace") { }

        public override string ProcessRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var trace = ""; //Logger.GetTrace();
            trace.Reverse();

            return Newtonsoft.Json.JsonConvert.SerializeObject(trace);
        }
    }
}
