
using Common;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using WebDownloader.Domain;
using WebDownloader.Domain.EventHandlers;
using WebDownloader.Services;

namespace WebDownloader.Downloaders
{
    public class YtDownloader : Downloader
    {
        private readonly string _bin;
        private readonly YtDownloadInfoParser _parser;
        public YtDownloader(string ytDownloaderBin) : base(DownloaderId)
        {
            _bin = ytDownloaderBin;//Environment.OSVersion.Platform == PlatformID.Win32NT ? @"C:\usr\bin\youtube-dl.exe" : @"/usr/local/bin/youtube-dl";
            _parser = new YtDownloadInfoParser();
        }
        public static string DownloaderId
        {
            get { return "youtube-dl";}
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
                    finfo.Directory?.Create();
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
            var finfo = new FileInfo(downloadInfo.OutputFile);
            if (finfo.Exists)
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
