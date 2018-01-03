using System.Collections.Generic;
using System.Net;
using System;
using System.Text;
using CloudFlareUtilities;
using System.Net.Http;

namespace JadeFlix.Services
{
    public class Web
    {
        private const string UserAgent = "Mozilla/5.0 (X11; U; Linux armv7l; en-US) AppleWebKit/534.16 (KHTML, like Gecko) Chrome/10.0.648.204 Safari/534.16";
        private const int MaxRetries = 5;
        private const int MaxParallelDownloads = 2;
        HttpClient _client;


        public Web()
        {
            var handler = new ClearanceHandler();
            _client = new HttpClient(handler);
        }

        public static CookieContainer CookieContainer { get; } = new CookieContainer();

        public string Get(string url, bool buffered = true)
        {
            return Get(new Uri(url), buffered);
        }

        public string Get(Uri url, bool buffered = true)
        {
            var result = _client.GetStringAsync(url).Result;
            return result;
        }
        public string PostJson(Uri url, string content)
        {
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            var httpResponse = _client.PostAsync(url, httpContent).Result;
            if (httpResponse.Content != null)
            {
                var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                return responseContent;
            }
            return string.Empty;
        }

        public string PostData(Uri url, IEnumerable<KeyValuePair<string,string>> content)
        {
            var httpContent = new FormUrlEncodedContent(content);

            var httpResponse = _client.PostAsync(url, httpContent).Result;
            if (httpResponse.Content != null)
            {
                var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                return responseContent;
            }
            return string.Empty;
        }
    }
}

