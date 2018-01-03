using Common.Logging;
using System;
using WebDownloader.Domain;
using WebDownloader.Services;

namespace WebDownloader.Downloaders
{
    public class CurlDownloadInfoParser : DownloadInfoLineParser
    {
        private int _linesToSkip;
        private int _skippedLines;
        public CurlDownloadInfoParser(int linesToSkip = 3)
        {
            _linesToSkip = linesToSkip;
            _skippedLines = 0;
        }

        public void Reset()
        {
            _linesToSkip = 0;
        }
        public override DownloadInfo Parse(string line, string id = null, Uri source = null, string file = null)
        {
            var data = new DownloadInfo(id, file, source);

            if (string.IsNullOrEmpty(line)) return data;
            if (_skippedLines < _linesToSkip)
            {
                _skippedLines++;
                return null;
            }
            var items = line.Split(" ", 7, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length < 7) return data;
            data.Id = id;
            data.BytesTotal = ConvertString.ToByteSize(items[1]);
            data.BytesReceived = ConvertString.ToByteSize(items[3]);
            data.BytesTransfered = ConvertString.ToByteSize(items[5]);

            Logger.Debug($"BytesTotal: {data.BytesTotal}");
            Logger.Debug($"BytesRecieved: {data.BytesReceived}");
            return data;
        }
    }
}
