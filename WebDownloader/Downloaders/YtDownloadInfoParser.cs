using Common.Logging;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WebDownloader.Domain;
using WebDownloader.Services;

namespace WebDownloader.Downloaders
{
    public class YtDownloadInfoParser : DownloadInfoLineParser
    {
        public YtDownloadInfoParser()
        {
        }

        public void Reset()
        {
        }

        public override DownloadInfo Parse(string line, string id = null, Uri source = null, string file = null)
        {
            var data = new DownloadInfo(id, file, source);

            if (string.IsNullOrEmpty(line) ||
                !line.Contains("%"))
            {
                return data;
            }
            Logger.Debug("Downloader Line: " + line);
            var items = line.Split(" ", 7, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length < 7) return data;
            data.Id = id;
            if (items[2] == "of")
            {
                data.BytesTotal = ConvertString.ToByteSize(items[3]);
            }
            else
            {
                data.BytesTotal = ConvertString.ToByteSize(items[2]);
            }
            var percentStr = items[1].Replace("%", "");
            if (percentStr != null && data.BytesTotal>0)
            {
                var percent = float.Parse(percentStr, NumberStyles.Number, new CultureInfo("en-US"));
                if (percent > 0)
                {
                    data.BytesReceived = (int)((data.BytesTotal * percent) / 100);
                }
                else
                {
                    data.BytesReceived = 0;
                }

            }
            Logger.Debug($"BytesTotal: {data.BytesTotal}");
            Logger.Debug($"BytesRecieved: {data.BytesReceived}");
            return data;
        }
    }
}
