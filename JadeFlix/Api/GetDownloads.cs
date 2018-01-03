using System.Net;
using SimpleWebApiServer;
using Newtonsoft.Json;
using System;
using System.Web;
using JadeFlix.Services;
using Common;
using System.IO;
using WebDownloader.Domain;
using System.Collections.Generic;

namespace JadeFlix.Api
{
    public class GetDownloads : ApiGetRequestResponse
    {
        public GetDownloads(HttpListenerRequestCache cache = null) : base("api/getDownloads",cache){}

        public override string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var downloads = AppContext.FileDownloader.GetDownloads();

            return JsonConvert.SerializeObject(downloads);
        }
        public override bool IsCacheable => false;
    }
}
