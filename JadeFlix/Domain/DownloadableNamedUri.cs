using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
