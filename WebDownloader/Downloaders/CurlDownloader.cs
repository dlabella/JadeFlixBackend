
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
    public class CurlDownloader : Downloader
    {
        string _curlBin;
        CurlDownloadInfoParser _curlDownloadInfoParser;
        public CurlDownloader() : base("Curl")
        {
            var bin = string.Empty;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                _curlBin = @"C:\usr\bin\curl.exe";
            }
            else
            {
                _curlBin = @"/usr/bin/curl";
            }
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

            string outputFilePath = filePath.ToSafePath();

            var downloadInfo = new CurlDownloadInfo(id, outputFilePath, url, cookies, disableTracking, downloadChanged, downloadCompleted);

            PrepareOutputDirectory(outputFilePath);

            Sys.RunProcess(
                _curlBin,
                downloadInfo.GetCommadArguments(),
                true,
                (data) => HandleDownloadFeedback(data, downloadInfo),
                (error) => HandleDownloadError(error),
                (exitCode) => HandleDownloadCompleted(exitCode, downloadInfo));
        }
        private void PrepareOutputDirectory(string filePath)
        {
            var finfo = new FileInfo(filePath);
            if (!finfo.Exists)
            {
                finfo.Directory.Create();
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
