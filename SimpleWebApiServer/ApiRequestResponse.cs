using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleWebApiServer
{
    public abstract class ApiRequestResponse
    {
        private const string URL_TOKEN_PATTERN_REGEX = "(\\{.*\\})";
        private readonly string _urlPattern;
        private List<int> _parameterIndexes = new List<int>();

        public ApiRequestResponse(string urlPattern, HttpListenerRequestCache cache=null)
        {
            _urlPattern = urlPattern;
            Cache = cache;
            var urlParts = urlPattern.Split("/");
            int i = 0;
            foreach(var part in urlParts)
            {
                if (part.StartsWith("{"))
                {
                    _parameterIndexes.Add(i);
                }
                i++;
            }
        }
        public abstract string HttpMethod { get; }
        public string UrlPattern
        {
            get { return _urlPattern; }
        }
        public static string StripPatternFromUrl(string url)
        {
            return Regex.Replace(url, URL_TOKEN_PATTERN_REGEX, "").ToLower();
        }

        public static string StripPatternFromUrl(Uri url)
        {
            return StripPatternFromUrl(url.ToString());
        }
        public virtual bool IsCacheable => true;
        public abstract string ProcessGetRequest(HttpListenerRequest request, RequestParameters parameters);
        public abstract string ProcessPostRequest(HttpListenerRequest request, RequestParameters parameters,string postData);
        public HttpListenerRequestCache Cache { get; set; }
    }
}
