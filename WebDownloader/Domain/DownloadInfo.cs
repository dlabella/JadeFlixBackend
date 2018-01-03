using Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace WebDownloader.Domain
{
    [Serializable]
    public class DownloadInfo
    {
        public DownloadInfo():this(string.Empty,null,false)
        {

        }
        public DownloadInfo(string file, Uri source):this(file,source,false)
        {

        }
        public DownloadInfo(string file, Uri source, bool isQueued):this(source?.ToString(),file,source,isQueued,false)
        {
        }
        public DownloadInfo(string id, string file, Uri source) : this(id, file, source, false, false)
        {
        }
        public DownloadInfo(string id, string file, Uri source, bool isQueued, bool disableTracking)
        {
            Id = id;
            File = file.CleanPath();
            Source = source;
            IsQueued = isQueued;
            DisableTracking = disableTracking;
        }
        [JsonIgnore]
        public bool DisableTracking { get; set; }
        DateTime _startDate = DateTime.MinValue;
        int _bytesReceived;
        [JsonProperty("name")]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(File)) return "Unknown";
                FileInfo finfo = new FileInfo(File);
                return finfo.Name;
            }
        }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("file")]
        public string File { get; set; }
        [JsonProperty("isQueued")]
        public bool IsQueued { get; set; }
        [JsonProperty("source")]
        public Uri Source { get; set; }
        [JsonProperty("bytesTotal")]
        public int BytesTotal { get; set; }
        [JsonProperty("bytesReceived")]
        public int BytesReceived
        {
            get { return _bytesReceived; }
            set
            {
                _bytesReceived = value;
                if (_bytesReceived > 0 && _startDate == DateTime.MinValue)
                {
                    _startDate = DateTime.Now;
                }
            }
        }
        [JsonProperty("bytesTransferred")]
        public int BytesTransfered { get; set; }
        [JsonProperty("timeSpent")]
        public TimeSpan TimeSpent
        {
            get { return (DateTime.Now - _startDate); }
        }
        [JsonProperty("isEmpty")]
        public bool IsEmpty { get; private set; }
        [JsonProperty("isCompleted")]
        public bool IsCompleted => (BytesReceived >= BytesTotal && BytesTotal > 0);
        [JsonProperty("percentCompleted")]
        public float PercentCompleted
        {
            get {
                if (BytesReceived>0 && BytesTotal > 0)
                {
                    return (((float)BytesReceived / (float)BytesTotal) * 100);
                }
                return 0;
            }
        }
        [JsonProperty("downloadFaulted")]
        public bool DownloadFaulted { get; set; }
        [JsonIgnore]
        public CookieCollection Cookies { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            return base.ToString();
        }
    }
}
