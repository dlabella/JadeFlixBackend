
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
    public class CurlDownloader : Downloader
    {
        private readonly string _curlBin;
        private readonly CurlDownloadInfoParser _curlDownloadInfoParser;
        public CurlDownloader() : base("Curl")
        {
            _curlBin = Environment.OSVersion.Platform == PlatformID.Win32NT ? @"C:\usr\bin\curl.exe" : @"/usr/bin/curl";
            _curlDownloadInfoParser = new CurlDownloadInfoParser();
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

            var outputFilePath = filePath.ToSafePath();

            var downloadInfo = new CurlDownloadInfo(id, outputFilePath, url, cookies, disableTracking, downloadChanged, downloadCompleted);

            PrepareOutputDirectory(outputFilePath);

            Sys.RunProcess(
                _curlBin,
                downloadInfo.GetCommadArguments(),
                true,
                (data) => HandleDownloadFeedback(data, downloadInfo),
                HandleDownloadError,
                (exitCode) => HandleDownloadCompleted(exitCode, downloadInfo));
        }
        private static void PrepareOutputDirectory(string filePath)
        {
            var finfo = new FileInfo(filePath);
            if (!finfo.Exists)
            {
                finfo.Directory?.Create();
            }
        }

        private void HandleDownloadFeedback(
            string line,
            CurlDownloadInfo downloadInfo
            )
        {
            var parsedData = _curlDownloadInfoParser.Parse(line, downloadInfo.Id, downloadInfo.Url, downloadInfo.OutputFile);
            if (parsedData == null || parsedData.IsEmpty)
            {
                Logger.Debug("Download Non Parseable: " + line);
                return;
            }
            downloadInfo.DownloadChanged?.Invoke(null, new DownloadCompletedEventArgs(parsedData));
        }

        private void HandleDownloadCompleted(int exitCode, CurlDownloadInfo downloadInfo)
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

        private static void HandleDownloadError(string line)
        {
            Logger.Exception("Download Error: " + line);
        }
    }
}
