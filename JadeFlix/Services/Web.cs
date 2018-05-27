using System.Net;
using System;
using Jadeflix.Services.Protections.CloudFare;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;

namespace JadeFlix.Services
{
    public class Web
    {
        private readonly HttpClient _client;

        public Web()
        {
            var handler = new ClearanceHandler();
            _client = new HttpClient(handler);
        }

        public static CookieContainer CookieContainer { get; } = new CookieContainer();

        public async Task<string> GetAsync(Uri url)
        {
            Logger.Debug($"Web GetAsync: {url}");
            var result = await _client.GetStringAsync(url);
            Logger.Debug($"Web Data Getted");
            return result;
        }
    }
}

