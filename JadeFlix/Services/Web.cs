using System.Net;
using System;
using Jadeflix.Services.Protections.CloudFare;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace JadeFlix.Services
{
    public class Web
    {
        private readonly HttpClient _client;
        private readonly HttpClient _insecureClient;
        public Web()
        {
            var handler = new ClearanceHandler();
            _client = new HttpClient(handler);
            _insecureClient = new HttpClient(new WebClientRetryHandler
            {
                ServerCertificateCustomValidationCallback = CertificateValidationCallback
            });
        }

        private bool CertificateValidationCallback(HttpRequestMessage arg1, X509Certificate2 arg2, X509Chain arg3, SslPolicyErrors arg4)
        {
            return true;
        }

        public static CookieContainer CookieContainer { get; } = new CookieContainer();

        public async Task<string> GetAsync(Uri url)
        {
            Logger.Debug($"Web GetAsync: {url}");
            var result = await _client.GetStringAsync(url);
            Logger.Debug($"Web Data Getted");
            return result;
        }

        public async Task<GetHeadersResponse> GetHeadersAsync(Uri url)
        {
            Logger.Debug($"Web GetHeadersAsync: {url}");

            var result = await _insecureClient.GetAsync(url,HttpCompletionOption.ResponseHeadersRead);
            var response = new GetHeadersResponse
            {
                Code = result.StatusCode,
                Headers = result.Headers
            };
            if (result.RequestMessage.RequestUri != null && result.RequestMessage.RequestUri!=url)
            {
                response.Code = HttpStatusCode.Redirect;
                response.Headers.Location = result.RequestMessage.RequestUri;
            }
            Logger.Debug($"Web Headers Getted");
            return response;
        }
    }
    public class GetHeadersResponse
    {
        public HttpStatusCode Code { get;set;}
        public HttpResponseHeaders Headers { get;set;}

    }


}

