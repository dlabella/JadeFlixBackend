using System;
using System.Net;
using System.Text;

namespace TvShows.Domain.SiteProtections
{
    public abstract class SiteProtection
    {
        protected SiteProtection(string name)
        {
            Name = name;
        }

        public string Name { get; internal set; }

        protected string BuildTraceStr(string message)
        {
            StringBuilder trace = new StringBuilder();
            trace.Append("[").Append(Name).Append("] ").Append(message);
            return trace.ToString();
        }

        public abstract Uri ProcessRequest(Uri baseUrl, WebRequest request, WebResponse response);

        protected string GetBaseUri(string url)
        {
            return GetBaseUri(new Uri(url));
        }

        protected string GetBaseUri(Uri url)
        {
            return url.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped);
        }

        public abstract bool IsActive(WebResponse response);
    }
}
