using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using WebDownloader.Domain;
using WebDownloader.Domain.EventHandlers;
using WebDownloader.Downloaders;
using Common;
//Reviewed: pending config for max parellel downloads
namespace JadeFlix.Services
{
    public class DownloadManager
    {
        private Queue<DownloadInfo> _queue = new Queue<DownloadInfo>();

        private static ConcurrentDictionary<string, DownloadInfo> _activeDownloads = new ConcurrentDictionary<string, DownloadInfo>();
       
        private const int MaxParallelDownloads = 2;

        CurlDownloader _downloader;

        public DownloadManager()
        {
            _downloader = new CurlDownloader();
        }

        public void Enqueue(string id, string filePath, Uri url, CookieContainer cookieContainer = null, bool forceDownload = false, bool disableTracking = false)
        {
            var di = BuildDownloadInfo(id, filePath, url, cookieContainer, true, disableTracking);
            if (EnqueueDownload(di, forceDownload))
            {
                ProcessQueue();
            }
            else
            {
                Logger.Debug("The url:" + url + " is already in queue or downloading...");
            }
        }

        private DownloadInfo BuildDownloadInfo(string id, string filePath, Uri url, CookieContainer cookieContainer = null, bool isQueued = true, bool disableTracking = false)
        {
            var di = new DownloadInfo(id, filePath, url, isQueued, disableTracking);

            if (cookieContainer != null)
            {
                di.Cookies = cookieContainer.GetCookies(url);
            }
            return di;
        }

        private bool EnqueueDownload(DownloadInfo di, bool forceDownload)
        {
            if (!_activeDownloads.ContainsKey(di.Id) || forceDownload)
            {
                _activeDownloads.AddOrUpdate(di.Id, di, (o, n) => di);

                _queue.Enqueue(di);
                return true;
            }
            return false;
        }

        public DownloadInfo DequeueDownload()
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Dequeue();
                item.IsQueued = false;
                return item;
            }
            return null;
        }

        public void ProcessQueue()
        {
            if (!CanProcessNextQueueItem())
            {
                return;
            }
            try
            {                
                Logger.Debug("Dequeuing download item ...");
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    var item = DequeueDownload();
                    ProcessDownload(item);
                });
            }
            catch (Exception ex)
            {
                Logger.Debug("Download Exception: " + ex.Message);
            }
        }
        private bool CanProcessNextQueueItem()
        {
            if (_queue.Count == 0) return false;
            if (_activeDownloads.Count > MaxParallelDownloads) return false;
            return true;
        }
        public IEnumerable<DownloadInfo> GetDownloads()
        {
            var active = new List<DownloadInfo>(_activeDownloads.Values);
            active.AddRange(_queue);
            return active;
        }
        private void ProcessDownload(DownloadInfo di)
        {
            _activeDownloads.AddOrUpdate(di.Id, di, (o, n) => di);

            _downloader.Download(di.Id, di.File.CleanPath(), di.Source, GetCookieDictionary(di.Cookies), di.DisableTracking,
               UpdateActiveDownload,
               ActiveDownloadCompleted
            );
        }

        private Dictionary<string,string> GetCookieDictionary(CookieCollection cookies)
        {
            var cookieDictionary = new Dictionary<string, string>();
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies)
                {
                    cookieDictionary.Add(cookie.Name, cookie.Value);
                }
            }
            return cookieDictionary;
        }

        private void ActiveDownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            _activeDownloads.TryRemove(e.Info.Id, out DownloadInfo val);

            ProcessQueue();

            Logger.Debug("Download of " + e.Info.File + " is completed");
        }

        private void UpdateActiveDownload(object sender, DownloadChangedEventArgs e)
        {
            _activeDownloads.AddOrUpdate(e.Info.Id, e.Info, (o, n) => e.Info);

            if (e.Info.BytesReceived > 0 && e.Info.BytesTotal > 0)
            {
                Logger.Debug(((e.Info.BytesReceived / e.Info.BytesTotal) * 100) + " - " + e.Info.File);
            }
            else
            {
                Logger.Debug("??% - " + e.Info.File);
            }
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
