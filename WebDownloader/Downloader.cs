using System;
using System.Collections.Generic;
using WebDownloader.Domain.EventHandlers;

namespace WebDownloader
{
    public abstract class Downloader
    {
        string _name;
        public Downloader(string name)
        {
            _name = name;
        }
        public string Name { get { return _name; } }

        public abstract void Download(
            string id,
            string filePath,
            Uri url,
            IEnumerable<KeyValuePair<string, string>> cookies,
            bool disableTracking,
            DownloadChangedEventHandler downloadChanged = null,
            DownloadCompletedEventHandler downloadCompleted = null);

    }
}
