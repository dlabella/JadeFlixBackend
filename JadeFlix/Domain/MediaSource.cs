using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JadeFlix.Domain
{
    [Serializable]
    public class MediaSource
    {
        [JsonProperty("remote")]

        public List<DownloadableNamedUri> Remote { get; set; } = new List<DownloadableNamedUri>();
        [JsonProperty("local")]

        public List<NamedUri> Local { get; set; } = new List<NamedUri>();
    }
}
