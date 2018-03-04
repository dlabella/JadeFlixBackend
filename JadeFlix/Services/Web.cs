using System.Collections.Generic;
using System.Net;
using System;
using System.Text;
using CloudFlareUtilities;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task<string> GetAsync(string url, bool buffered = true)
        {
            return await GetAsync(new Uri(url), buffered);
        }

        public async Task<string> GetAsync(Uri url, bool buffered = true)
        {
            var result = await _client.GetStringAsync(url);
            return result;
        }
        public async Task<string> PostJson(Uri url, string content)
        {
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            var httpResponse = await _client.PostAsync(url, httpContent);
            if (httpResponse.Content != null)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                return responseContent;
            }
            return string.Empty;
        }

        public async Task<string> PostDataAsync(Uri url, IEnumerable<KeyValuePair<string,string>> content)
        {
            var httpContent = new FormUrlEncodedContent(content);

            var httpResponse = await _client.PostAsync(url, httpContent);
            if (httpResponse.Content != null)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                return responseContent;
            }
            return string.Empty;
        }
    }
}

