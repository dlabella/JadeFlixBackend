using Newtonsoft.Json;
using System;

namespace JadeFlix.Domain
{
    [Serializable]
    public class DownloadableNamedUri:NamedUri
    {
        [JsonProperty("downloadPercentCompleted")]

        public int DownloadPercentCompleted { get; set; }
        [JsonProperty("downloading")]

        public bool Downloading { get; set; }
    }
}
