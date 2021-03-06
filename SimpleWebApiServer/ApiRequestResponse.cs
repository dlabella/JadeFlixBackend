﻿using Newtonsoft.Json;
using SimpleWebApiServer.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SimpleWebApiServer
{
    public abstract class ApiRequestResponse<TParams> : IApiRequestResponse
    {
        private readonly List<int> _parameterIndexes = new List<int>();

        protected ApiRequestResponse(string urlPattern, HttpListenerRequestCache cache=null)
        {
            UrlPattern = urlPattern;
            Cache = cache;
            var urlParts = urlPattern.Split("/");
            var i = 0;
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
        public string UrlPattern { get; }

        public virtual bool IsCacheable => true;
        public async Task<string> GetRequestAsync(HttpListenerRequest request, RequestParameters parameters)
        {
            var qryParams = ParseParameters(parameters);
            return await ProcessGetRequest(request, qryParams);
        }
        protected abstract Task<string> ProcessGetRequest(HttpListenerRequest request, TParams parameters);
        public async Task<string> PostRequestAsync(HttpListenerRequest request, RequestParameters parameters,string postData)
        {
            var qryParams = ParseParameters(parameters);
            return await ProcessPostRequest(request, qryParams, postData);
        }
        protected abstract Task<string> ProcessPostRequest(HttpListenerRequest request, TParams parameters, string postData);
        public HttpListenerRequestCache Cache { get; set; }

        public string ToJson(object obj)
        {
            if (obj==null) {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj);
        }

        public T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        protected abstract TParams ParseParameters(RequestParameters parameters);
    }
}
