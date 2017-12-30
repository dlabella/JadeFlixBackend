using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using TvShows.Domain.SiteProtections;
using System;
using System.Text;
using System.IO;
using MediaCatalog.Services.Protections;
using Common.Logging;

namespace JadeFlix.Services
{
    public class Web
    {
        private const string UserAgent = "Mozilla/5.0 (X11; U; Linux armv7l; en-US) AppleWebKit/534.16 (KHTML, like Gecko) Chrome/10.0.648.204 Safari/534.16";
        private const int MaxRetries = 5;
        private const int MaxParallelDownloads = 2;
        private readonly List<SiteProtection> _protections = new List<SiteProtection>();
        WebClient _client;
        public Web()
        {
            _protections.Add(new RedirectProtection());
            _protections.Add(new CloudFare());

            _client = new WebClient();
        }

        public static CookieContainer CookieContainer { get; } = new CookieContainer();

        public string Read(string url, bool buffered = true)
        {
            return Read(new Uri(url), buffered);
        }

        public string Read(Uri url, bool buffered = true)
        {
            return Send(url);
        }
        public string PostJson(Uri url, string content, bool buffered = true)
        {
            return Send(url, "POST", "application/json", content, buffered);
            //application/x-www-form-urlencoded; charset=UTF-8
        }

        public string PostData(Uri url, string content, bool buffered = true)
        {
            return Send(url, "POST", "application/x-www-form-urlencoded; charset=UTF-8", content, buffered);
        }

        public string Send(Uri url, string method=null, string contentType=null, string content=null, bool buffered = true)
        {
            var retry = false;
            var retriesLeft = MaxRetries;
            var result = string.Empty;
            var urlToRead = url;
            Logger.Debug("Reading url: " + url);
            do
            {
                var request = WebRequest.CreateHttp(urlToRead.ToString()) as HttpWebRequest;
                try
                {
                    if (method != null)
                    {
                        request.Method = method;
                    }
                    if (contentType != null && content != null)
                    {
                        request.ContentType = contentType;
                    }
                    if (!string.IsNullOrEmpty(content))
                    {
                        using (var req = request.GetRequestStream())
                        {
                            var data = Encoding.UTF8.GetBytes(content);
                            req.Write(data, 0, data.Length);
                        }
                    }
                    result = ProcessRequest(request, buffered);
                    retry = false;
                }
                catch (WebException wex)
                {
                    Logger.Debug("Request failed...");
                    Logger.Debug("...");
                    try
                    {
                        urlToRead = ProcessProtections(urlToRead, request, wex);
                        retry = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Exception: " + ex.Message);
                        retry = false;
                    }

                    if (!retry)
                    {
                        Trace.TraceError(wex.ToString());
                        throw;
                    }
                    retriesLeft--;
                }
            } while (retry && retriesLeft > 0);

            return result;
        }

        private static string ProcessRequest(HttpWebRequest request, bool buffered = false)
        {
            Trace.WriteLine("Web Requesting: " + request.RequestUri);

            request.CookieContainer = CookieContainer;
            request.AllowReadStreamBuffering = buffered;
            request.Timeout = 20000;
            request.ReadWriteTimeout = 20000;
            request.UserAgent = UserAgent;
            request.AllowAutoRedirect = true;
            Trace.WriteLine($"Requesting data to: {request.RequestUri}");

            using (var response = request.GetResponse())
            {
                Trace.WriteLine("Processing response stream");
                using (var stm = response.GetResponseStream())
                {
                    return ReadUtf8Stream(stm);
                }
            }
        }

        private static string ReadUtf8Stream(Stream stm, int bufferSize=2048)
        {
            var buffer = new byte[bufferSize];
            int readCount = 0;
            var result = new StringBuilder();
            if (stm == null) return string.Empty;
            do
            {
                readCount = stm.Read(buffer, 0, buffer.Length);
                if (readCount > 0)
                {
                    result.Append(Encoding.UTF8.GetString(buffer, 0, readCount));
                }
            } while (readCount == buffer.Length);

            return result.ToString();
        }

        private Uri ProcessProtections(Uri url, WebRequest request, WebException ex)
        {
            foreach (var protection in _protections)
            {
                if (!protection.IsActive(ex.Response))
                {
                    Logger.Debug($"Protection {protection.Name} is not enabled");
                    continue;
                }

                Logger.Debug("Processing protection: " + protection.GetType().Name);
                return protection.ProcessRequest(url, request, ex.Response);
            }

            Logger.Debug(ex.StackTrace);
            return new Uri(string.Empty);
        }
    }
}

