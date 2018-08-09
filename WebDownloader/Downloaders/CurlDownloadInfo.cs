using System;
using System.Collections.Generic;
using System.Text;
using WebDownloader.Domain.EventHandlers;

namespace WebDownloader.Downloaders
{
    public class CurlDownloadInfo
    {
        public CurlDownloadInfo(
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
            AddUrlToCommand(argString, Url);
            AddCookiesToCommand(argString, Cookies);
            AddOutputFileToCommand(argString, OutputFile);
            return argString.ToString();
        }

        
        private void AddUrlToCommand(StringBuilder sb, Uri url)
        {
            sb.Append(url);
        }
        private void AddOutputFileToCommand(StringBuilder sb, string filePath)
        {
            sb.Append(" -o --insecure \"")
              .Append(filePath)
              .Append("\" ");
        }
        private void AddCookiesToCommand(StringBuilder sb, IEnumerable<KeyValuePair<string, string>> cookies)
        {
            if (cookies != null)
            {
                sb.Append(" ");
                foreach (var cookie in cookies)
                {
                    sb.Append("--cookie \"" + cookie.Key + "=" + cookie.Value + "\" ");
                }
                sb.Append(" ");
            }
        }
    }
}
