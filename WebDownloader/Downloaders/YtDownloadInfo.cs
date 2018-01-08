using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebDownloader.Domain.EventHandlers;

namespace WebDownloader.Downloaders
{
    public class YtDownloadInfo
    {
        public YtDownloadInfo(
            string id,
            string outputFile,
            Uri url,
            IEnumerable<KeyValuePair<string, string>> cookies = null,
            bool disableTracking = false,
            DownloadChangedEventHandler downloadChanged = null,
            DownloadCompletedEventHandler downloadCompleted = null)
        {
            Id = id;
            OutputFile = outputFile;
            Url = url;
            Cookies = cookies;
            DisableTracking = disableTracking;
            DownloadChanged = downloadChanged;
            DownloadCompleted = downloadCompleted;
        }

        public string Id { get; set; }
        public string OutputFile { get; set; }
        public Uri Url { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Cookies { get; set; }
        public bool DisableTracking { get; set; }
        public DownloadChangedEventHandler DownloadChanged { get; set; }
        public DownloadCompletedEventHandler DownloadCompleted { get; set; }

        public string GetCommadArguments()
        {
            var argString = new StringBuilder();
            AddNewLineToCommand(argString);
            AddCookiesToCommand(argString, Cookies);
            AddOutputFileToCommand(argString, OutputFile);
            AddUrlToCommand(argString, Url);
            return argString.ToString();
        }

        private void AddNewLineToCommand(StringBuilder sb)
        {
            sb.Append(" --newline ");
        }
        private void AddUrlToCommand(StringBuilder sb, Uri url)
        {
            sb.Append(url);
        }
        private void AddOutputFileToCommand(StringBuilder sb, string filePath)
        {
            sb.Append(" -o \"")
              .Append(filePath)
              .Append("\" ");
        }
        private void AddCookiesToCommand(StringBuilder sb, IEnumerable<KeyValuePair<string, string>> cookies)
        {
            if (cookies != null)
            {
                sb.Append("--add-header Cookie:\"");
                foreach (var cookie in cookies)
                {
                    sb.Append(cookie.Key + "=" + cookie.Value + ";");
                }
                sb.Append("\"");
            }
        }
    }
}
