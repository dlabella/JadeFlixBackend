using Common.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using WebDownloader.Domain;
using WebDownloader.Domain.EventHandlers;
using WebDownloader.Downloaders;
using Common;
using System.Linq;

namespace JadeFlix.Services
{
    public class DownloadManager
    {
        private readonly ConcurrentQueue<DownloadInfo> _queue = new ConcurrentQueue<DownloadInfo>();
        private static readonly ConcurrentDictionary<string, DownloadInfo> ActiveDownloads = new ConcurrentDictionary<string, DownloadInfo>();
        private const int MaxParallelDownloads = 2;
        private readonly YtDownloader _downloader;

        public DownloadManager()
        {
            var bin = AppContext.Config.Downloaders[YtDownloader.DownloaderId];
            _downloader = new YtDownloader(bin);
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

        private static DownloadInfo BuildDownloadInfo(string id, string filePath, Uri url, CookieContainer cookieContainer = null, bool isQueued = true, bool disableTracking = false)
        {
            var di = new DownloadInfo(id, filePath, url, isQueued, disableTracking);

            if (cookieContainer != null)
            {
                di.Cookies = cookieContainer.GetCookies(url);
            }
            return di;
        }

        private bool EnqueueDownload(DownloadInfo downloadInfo, bool forceDownload)
        {
            if (_queue.Any(x => x.Id == downloadInfo.Id) ||
                ActiveDownloads.ContainsKey(downloadInfo.Id) && !forceDownload)
            {
                return false;
            }
            _queue.Enqueue(downloadInfo);
            return true;
        }

        private DownloadInfo DequeueDownload()
        {
            if (_queue.Count <= 0)
            {
                return null;
            }
            _queue.TryDequeue(out var item);
            item.IsQueued = false;
            return item;
        }

        private void ProcessQueue()
        {
            if (!CanProcessNextQueueItem())
            {
                Logger.Debug("Cannot process next item");
                Logger.Debug("Queue Count: " + _queue.Count);
                Logger.Debug("Active downloads: " + ActiveDownloads.Values.Count(x => !x.IsQueued));
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
                Logger.Exception("Download Exception: " + ex.Message,ex);
            }
        }
        private bool CanProcessNextQueueItem()
        {
            if (_queue.Count == 0) return false;
            return ActiveDownloads.Values.Count(x => !x.IsQueued) < MaxParallelDownloads;
        }
        public IEnumerable<DownloadInfo> GetDownloads()
        {
            var active = new List<DownloadInfo>(ActiveDownloads.Values);
            active.AddRange(_queue);
            return active;
        }
        private void ProcessDownload(DownloadInfo di)
        {
            ActiveDownloads.AddOrUpdate(di.Id, di, (o, n) => di);

            _downloader.Download(di.Id, di.File.ToSafePath(), di.Source, GetCookieDictionary(di.Cookies), di.DisableTracking,
               UpdateActiveDownload,
               ActiveDownloadCompleted
            );
        }

        private static Dictionary<string, string> GetCookieDictionary(IEnumerable cookies)
        {
            var cookieDictionary = new Dictionary<string, string>();
            if (cookies == null)
            {
                return cookieDictionary;
            }
            foreach (Cookie cookie in cookies)
            {
                cookieDictionary.Add(cookie.Name, cookie.Value);
            }
            return cookieDictionary;
        }

        private void ActiveDownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            ActiveDownloads.TryRemove(e.Info.Id, out _);
            ProcessQueue();

            Logger.Debug("Download of " + e.Info.File + " is completed");
        }

        private static void UpdateActiveDownload(object sender, DownloadChangedEventArgs e)
        {
            ActiveDownloads.AddOrUpdate(e.Info.Id, e.Info, (o, n) => e.Info);

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
}
