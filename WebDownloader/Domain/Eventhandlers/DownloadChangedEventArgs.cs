using System;

namespace WebDownloader.Domain.EventHandlers
{
    public class DownloadChangedEventArgs : EventArgs
    {
        public DownloadInfo Info { get; private set; }
        public DownloadChangedEventArgs(DownloadInfo e)
        {
            Info = e;
        }
    }
}
