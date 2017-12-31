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
    public class GetDownloads : ApiRequestResponse
    {
        public GetDownloads() : base("api/getDownloads"){}

        public override string ProcessRequest(HttpListenerRequest request, RequestParameters parameters)
        {
            var downloads = AppContext.FileDownloader.GetDownloads();

            return JsonConvert.SerializeObject(downloads);
        }
        public override bool IsCacheable => false;
    }
}
