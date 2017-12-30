using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using WebDownloader.Domain;
using WebDownloader.Downloaders;

namespace JadeFlix.Services
{
    public class DownloadManager
    {
        private Queue<DownloadInfo> _queue = new Queue<DownloadInfo>();

        private static ConcurrentDictionary<string, DownloadInfo> _activeDownloads = new ConcurrentDictionary<string, DownloadInfo>();
        private static ConcurrentDictionary<string, DownloadInfo> _completedDownloads = new ConcurrentDictionary<string, DownloadInfo>();

        private const int MaxParallelDownloads = 2;

        CurlDownloader _downloader;
        public DownloadManager()
        {
            
            _downloader = new CurlDownloader();
        }
        public void Enqueue(string id, string filePath, Uri url, CookieContainer cookieContainer = null, bool forceDownload = false, bool removeOnCompletion = true)
        {
            var di = new DownloadInfo(id, filePath, url, true, removeOnCompletion);

            if (cookieContainer != null)
            {
                di.Cookies = cookieContainer.GetCookies(url);
            }
            if (!_activeDownloads.ContainsKey(id)|| forceDownload)
            {   
                _activeDownloads.AddOrUpdate(id, di, (o, n) => di);

                _queue.Enqueue(di);

                if (_activeDownloads.Count < MaxParallelDownloads)
                {
                    ProcessQueue();
                }
            }
            else
            {
                Logger.Debug("The url:" + url + " is already downloading...");
            }
        }
        public void ProcessQueue()
        {
            if (_queue.Count == 0) return;
            try
            {
                Logger.Debug("Dequeuing download item ...");
                var item = _queue.Dequeue();
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    Download(item.Id, item.File, item.Source, item.Cookies);
                });
            }
            catch (Exception ex)
            {
                Logger.Debug("Download Exception: " + ex.Message);
            }
        }
        public IEnumerable<DownloadInfo> GetActiveDownloads()
        {
            var active = new List<DownloadInfo>(_activeDownloads.Values);
            active.AddRange(_completedDownloads.Values);
            return active;
        }
        private void Download(string id, string filePath, Uri url, CookieCollection cookieContainer)
        {
            var cookies = new Dictionary<string, string>();
            if (cookieContainer != null)
            {
                foreach (Cookie cookie in cookieContainer)
                {
                    cookies.Add(cookie.Name, cookie.Value);
                }
            }

            var safePath = Common.String.CleanPath(filePath);

            _activeDownloads.AddOrUpdate(id, new DownloadInfo(filePath, url, false), (o, n) => new DownloadInfo(filePath, url, false));

            _downloader.Download(id, safePath, url, cookies,
               (s, x) =>
               {
                   _activeDownloads.AddOrUpdate(x.Info.Id, x.Info, (o, n) => x.Info);

                   if (x.Info.BytesReceived > 0 && x.Info.BytesTotal > 0)
                   {
                       Logger.Debug(((x.Info.BytesReceived / x.Info.BytesTotal) * 100) + " - " + x.Info.File);
                   }
                   else
                   {
                       Logger.Debug("??% - " + x.Info.File);
                   }
               },
               (s, x) =>
               {
                   _activeDownloads.TryRemove(x.Info.Id, out DownloadInfo val);

                   x.Info.BytesReceived = x.Info.BytesTotal;
                   if (!x.Info.RemoveOnCompletion)
                   {
                       _completedDownloads.AddOrUpdate(x.Info.Id, val, (o, n) => x.Info);
                   }
                   Logger.Debug("Download of " + x.Info.File + " is completed");

                   ProcessQueue();
               }
            );
        }
        public void RemoveCompletedDownload(Uri url)
        {
            RemoveCompletedDownload(url.ToString());
        }
        public void RemoveCompletedDownload(string id)
        {
            _completedDownloads.TryRemove(id, out DownloadInfo download);
            download = null;
        }
    }
    internal class DownloadItem
    {
        public string Id { get; set; }
        public string FilePath { get; set; }
        public Uri Url { get; set; }
        public CookieCollection Cookies { get; set; }
    }
}
