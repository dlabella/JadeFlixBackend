using Common.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JadeFlix.Services
{
    public class WebClientRetryHandler : HttpClientHandler
    {
        private readonly int MaxRetries;
        public WebClientRetryHandler() : this(3)
        {
        }
        public WebClientRetryHandler(int maxRetries)
        {
            MaxRetries = maxRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    response = await SendRequestAsync(request, cancellationToken);
                    ProcessRequestResponse(request, i, response);
                }
                catch (Exception ex)
                {
                    ProcessRequestException(request, i, ex);
                }
            }

            return response;
        }


        private void ProcessRequestResponse(HttpRequestMessage request, int currentRetry, HttpResponseMessage response = null)
        {
            if (response != null && response.IsSuccessStatusCode)
            {
                if (currentRetry < MaxRetries)
                {
                    Logger.Debug($"Unsuccessfull response when request to [{request.RequestUri}] failed, retrying");
                }
            }
        }

        private void ProcessRequestException(HttpRequestMessage request, int currentRetry, Exception ex = null)
        {

            if (currentRetry < MaxRetries)
            {
                Logger.Debug($"Unsuccessfull response when request to [{request.RequestUri}] failed, retrying");
            }
            else
            {
                Logger.Exception($"****************************************");
                Logger.Exception($"Request to [{request.RequestUri}] failed");
                if (ex is AggregateException aggex)
                {
                    foreach (var innerEx in aggex.InnerExceptions)
                    {
                        Logger.Exception($"", ex.InnerException);
                    }
                }
                else
                {
                    Logger.Exception($"", ex);
                }
                Logger.Exception($"****************************************");
            }
        }

        protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
