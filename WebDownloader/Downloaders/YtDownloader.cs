
using Common;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebDownloader.Domain;
using WebDownloader.Domain.EventHandlers;
using WebDownloader.Services;

namespace WebDownloader.Downloaders
{
    public class YtDownloader : Downloader
    {
        string _bin;
        YtDownloadInfoParser _parser;
        public YtDownloader() : base("youtube-dl")
        {
            var bin = string.Empty;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                _bin = @"C:\usr\bin\youtube-dl.exe";
            }
            else
            {
                _bin = @"/usr/local/bin/youtube-dl";
            }
            _parser = new YtDownloadInfoParser();
        }

        public override void Download(
            string id,
            string filePath,
            Uri url,
            IEnumerable<KeyValuePair<string, string>> cookies,
            bool disableTracking,
            DownloadChangedEventHandler downloadChanged = null,
            DownloadCompletedEventHandler downloadCompleted = null)
        {

            string outputFilePath = filePath.ToSafePath();
            var downloadInfo = new YtDownloadInfo(id, outputFilePath, url, cookies, disableTracking, downloadChanged, downloadCompleted);
            if (PrepareOutputDirectory(outputFilePath))
            {
                Sys.RunProcess(
                    _bin,
                    downloadInfo.GetCommadArguments(),
                    true,
                    (data) => HandleDownloadFeedback(data, downloadInfo),
                    (error) => HandleDownloadError(error),
                    (exitCode) => HandleDownloadCompleted(exitCode, downloadInfo));
            }
        }
        private bool PrepareOutputDirectory(string filePath)
        {
            try
            {
                var finfo = new FileInfo(filePath);
                if (!finfo.Exists)
                {
                    finfo.Directory.Create();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void HandleDownloadFeedback(
            string line,
            YtDownloadInfo downloadInfo
            )
        {
            var parsedData = _parser.Parse(line, downloadInfo.Id, downloadInfo.Url, downloadInfo.OutputFile);
            if (parsedData == null || parsedData.IsEmpty)
            {
                Logger.Debug("Download Non Parseable: " + line);
                return;
            }
            downloadInfo.DownloadChanged?.Invoke(null, new DownloadCompletedEventArgs(parsedData));
        }

        private void HandleDownloadCompleted(int exitCode, YtDownloadInfo downloadInfo)
        {
            var di = new DownloadInfo(downloadInfo.Id, downloadInfo.OutputFile, downloadInfo.Url);
            FileInfo finfo = new FileInfo(downloadInfo.OutputFile);
            if (finfo != null && finfo.Exists)
            {
                di.BytesTotal = (int)finfo.Length;
                di.BytesReceived = di.BytesTotal;
            }
            else
            {
                di.BytesTotal = 1;
                di.BytesReceived = 1;
            }
            Logger.Debug("Download exit code: " + exitCode);
            di.DownloadFaulted = (exitCode != 0);
            downloadInfo.DownloadCompleted?.Invoke(null, new DownloadCompletedEventArgs(di));
        }

        private void HandleDownloadError(string line)
        {
            Logger.Debug("Download Error: " + line);
        }
    }
}
