using System;
using System.Collections.Generic;

namespace SimpleWebApiServer
{
    internal class Requesturl
    {
        private readonly string _baseUrl;

        public Requesturl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public bool PatternMatch(string url, string pattern)
        {
            return PatternMatch(_baseUrl, url, pattern);
        }

        private static bool PatternMatch(string baseUrl, string url, string pattern)
        {
            var strippedUrl = url;
            if (url.Contains("?"))
            {
                var start = url.IndexOf("?", StringComparison.Ordinal);
                if (start > 0)
                {
                    strippedUrl = url.Substring(0, start);
                }
            }

            var splittedUrl = strippedUrl.Replace(baseUrl, "").TrimEnd('/').Split("/");
            var splittedPattern = pattern.Replace(baseUrl, "").TrimEnd('/').Split("/");
            if (splittedUrl.Length != splittedPattern.Length) return false;
            for (var i = 0; i < splittedUrl.Length; i++)
            {
                if (splittedPattern[i].StartsWith("{")) continue;
                if (string.Compare(splittedUrl[i], splittedPattern[i], StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public Dictionary<string, string> GetParametersFromUrl(string url, string pattern)
        {
            return GetParametersFromUrl(_baseUrl, url, pattern);
        }

        private static Dictionary<string, string> GetParametersFromUrl(string baseUrl, string url, string pattern)
        {
            var strippedUrl = url;
            var start = url.LastIndexOf("/", StringComparison.Ordinal);
            if (start > 0 && url.Substring(start).Contains("?"))
            {
                strippedUrl = url.Substring(0, url.IndexOf("?", StringComparison.Ordinal));
            }
            var splittedUrl = strippedUrl.Replace(baseUrl, "").TrimEnd('/').Split("/");
            var splittedPattern = pattern.Replace(baseUrl, "").TrimEnd('/').Split("/");
            var result = new Dictionary<string, string>();

            for (var i = 0; i < splittedPattern.Length; i++)
            {
                if (splittedUrl.Length > i &&
                    splittedPattern[i].StartsWith("{"))
                {
                    result.Add(splittedPattern[i].Replace("{", "").Replace("}", ""), splittedUrl[i]);
                }
            }

            return result;
        }

        public static IDictionary<string, string> GetQueryParametersFromUrl(string url)
        {
            var result = new Dictionary<string, string>();

            var start = url.IndexOf("?", StringComparison.Ordinal);
            if (start < 0)
            {
                return result;
            }
            var queryString = url.Substring(start + 1);
            foreach (var p in queryString.Split("&"))
            {
                if (p.Contains("="))
                {
                    var value = p.Split("=", 2, StringSplitOptions.RemoveEmptyEntries);
                    result.Add(value[0], value[1]);
                }
                else
                {
                    result.Add(p, string.Empty);
                }
            }
            return result;
        }
    }
}