using System;

namespace WebDownloader.Domain
{
    public abstract class DownloadInfoLineParser
    {
        public abstract DownloadInfo Parse(string line,string id=null, Uri source = null, string file=null);
    }
}
