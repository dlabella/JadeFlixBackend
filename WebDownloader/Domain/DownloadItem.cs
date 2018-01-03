
using System;
using System.Collections.Generic;
using WebDownloader.Domain.EventHandlers;

namespace WebDownloader.Domain
{
    public class DownloadItem
    {
        public DownloadItem(string downloaderName, string file, Uri url, IEnumerable<KeyValuePair<string, string>> cookies, 
            DownloadChangedEventHandler downloadChanged=null,
            DownloadCompletedEventHandler downloadCompleted=null)
        {
            DownloaderName = downloaderName;
            File = file;
            Url = url;
            Cookies = cookies;
            DownloadChanged = downloadChanged;
            DownloadCompleted = downloadCompleted;
            DownloadStatus = new DownloadInfo(file, url);
        }
        public string DownloaderName { get; private set; }
        public string File { get; private set; }
        public Uri Url { get; private set; }
        public IEnumerable<KeyValuePair<string, string>> Cookies { get; private set; }
        public DownloadChangedEventHandler DownloadChanged { get; private set; }
        public DownloadCompletedEventHandler DownloadCompleted { get; private set; }
        public DownloadInfo DownloadStatus { get; set; }
    }
}
