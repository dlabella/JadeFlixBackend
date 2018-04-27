﻿using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Diagnostics;
using System;
using Common.Logging;
using System.IO;
using System.Linq;
using SimpleWebApiServer.Interfaces;
using System.Threading.Tasks;

namespace SimpleWebApiServer
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private static HttpListenerRequestCache _requestCache;
        private List<IApiRequestResponse> _requestHandlers;
        private Requesturl _urlParser;

        private string _urlPrefix;

        public WebServer(string ip, int port, string urlPrefix = null)
        {
            
            if (!string.IsNullOrEmpty(urlPrefix))
            {
                if (!urlPrefix.EndsWith("/"))
                {
                    _urlPrefix = urlPrefix + "/";
                }
            }
            _requestCache = new HttpListenerRequestCache(_urlPrefix);

            string prefix = "http://" + ip + ":" + port + "/";

            Logger.Debug("Listening on: " + prefix);

            Logger.Debug("Url Prefix: " + _urlPrefix);

            _requestHandlers = new List<IApiRequestResponse>();

            _urlParser = new Requesturl(prefix);

            _listener.Prefixes.Add(prefix);

            _listener.Start();
        }

        public HttpListenerRequestCache Cache { get { return _requestCache; } }

        public void RegisterRequestHandler(IApiRequestResponse requestHandler)
        {
            Logger.Debug("Registering module for pattern : " + requestHandler.UrlPattern);
            _requestHandlers.Add(requestHandler);
            if (!requestHandler.IsCacheable)
            {
                _requestCache.AddNonCacheableRequest(requestHandler.UrlPattern);
            }
        }

        private async Task<string> HandleRequest(HttpListenerRequest request)
        {
            foreach (var requestHandler in _requestHandlers.Where(x => x.HttpMethod.Contains(request.HttpMethod)))
            {
                var path = JoinUrlPath(_urlPrefix, requestHandler.UrlPattern);
                if (!_urlParser.PatternMatch(request.Url.ToString(), path))
                {
                    continue;
                }
                var requestParameters = GetRequestParameters(request, path);
                switch (request.HttpMethod)
                {
                    case "POST":
                        var postData = await GetPostDataAsync(request);
                        return await requestHandler.PostRequestAsync(request, requestParameters, postData);
                    case "GET":
                        return await requestHandler.GetRequestAsync(request, requestParameters);
                }
            }
            return Responses.NotFound(request, new RequestParameters());
        }

        private string JoinUrlPath(string a, string b)
        {
            if (a.EndsWith("/") && b.StartsWith("/"))
            {
                return a.Substring(0, a.Length - 1) + b;
            }
            return a + b;
        }

        private void OptionsRequestResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request == null)
            {
                return;
            }

            response.Headers.Clear();
            SetCorsHeaders(response);
            response.StatusCode = 200;
        }

        private void SetCorsHeaders(HttpListenerResponse response)
        {
            response.AppendHeader("Access-Control-Allow-Origin", "*");
            response.AppendHeader("Access-Control-Allow-Methods", "POST, GET, PUT, DELETE, OPTIONS");
            response.AppendHeader("Access-Control-Allow-Credentials", "false");
            response.AppendHeader("Access-Control-Max-Age", "86400");
            response.AppendHeader("Access-Control-Allow-Headers", "*");
        }
        private async Task<string> GetPostDataAsync(HttpListenerRequest request)
        {
            string data = string.Empty;
            try
            {
                if (request.HasEntityBody)
                {
                    using (var rdr = new StreamReader(request.InputStream, Encoding.UTF8))
                    {
                        data = await rdr.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Can't get post data!, ex:" + ex.Message);
                data = string.Empty;
            }
            return data;
        }
        private RequestParameters GetRequestParameters(HttpListenerRequest request, string urlPattern)
        {
            return new RequestParameters(_urlParser.GetParametersFromUrl(request.Url.ToString(), urlPattern),
                                                                  Requesturl.GetQueryParametersFromUrl(request.Url.ToString()));
        }

        public async void Run()
        {
           while (_listener.IsListening)
           {
                var ctx = await _listener.GetContextAsync();
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    if (string.Compare(ctx.Request.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase) == 0 ||
                        string.Compare(ctx.Request.HttpMethod, "ORIGIN", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OptionsRequestResponse(ctx.Request, ctx.Response);
                    }
                    else if (!ctx.Request.RawUrl.EndsWith("favicon.ico"))
                    {
                        var responseStr = await _requestCache.GetRequest(ctx.Request, (req) => HandleRequest(req));
                        sw.Stop();
                        var buf = Encoding.UTF8.GetBytes(responseStr);

                        ctx.Response.AddHeader("Content-Type", "application/json");
                        SetCorsHeaders(ctx.Response);
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentLength64 = buf.Length;
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                    }
                }
                catch (AggregateException agex)
                {
                    foreach (var ex in agex.InnerExceptions)
                    {
                        Logger.Debug("EXCEPTION: " + ex.Message);
                    }
                    //var errorResponse = Responses.InternalServerError(ctx.Request, GetRequestParameters(ctx.Request), ex);
                    //var errorBuf = Encoding.UTF8.GetBytes(errorResponse);
                    //ctx.Response.ContentLength64 = errorBuf.Length;
                    //ctx.Response.OutputStream.Write(errorBuf, 0, errorBuf.Length);
                }
                finally
                {
                    ctx.Response.OutputStream.Close();
                    ctx.Response.Close();
                }
            }
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        ~WebServer()
        {
            Stop();
        }
    }
}