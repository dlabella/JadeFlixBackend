using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TvShows.Domain.SiteProtections;

namespace MediaCatalog.Services.Protections
{
    public class RedirectProtection : SiteProtection
    {
        public RedirectProtection() : base("RedirectProtection")
        {
        }

        public override bool IsActive(WebResponse response)
        {
            var webResponse = response as HttpWebResponse;
            return (webResponse != null && webResponse.StatusCode == HttpStatusCode.MovedPermanently);
        }

        public override Uri ProcessRequest(Uri baseUrl, WebRequest request, WebResponse response)
        {
            foreach(var header in response.Headers.AllKeys)
            {
                Console.WriteLine($"{header} = " + response.Headers[header]);
            }
            return new Uri(response.Headers[HttpResponseHeader.ContentLocation]);
        }
    }
}
