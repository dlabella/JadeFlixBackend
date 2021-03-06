﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JadeFlix.Domain
{
    [Serializable]
    public class CatalogItemOv1
    {
        public CatalogItemOv1()
        {
            Episodes = new Episodes();
        }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("url")]
        public string Url { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("plot")]
        public string Plot { get; set; }
        
        [JsonProperty("banner")]
        public CatalogImage Banner { get; set; }
        [JsonProperty("poster")]
        public CatalogImage Poster { get; set; }
        [JsonProperty("scrapedBy")]
        public string ScrapedBy { get; set; }
        [JsonProperty("episodes")]
        public Episodes Episodes { get; set; }
    }
    [Serializable]
    public class Episodes
    {
        public Episodes()
        {
            Available = new List<DownloadableNamedUri>();
        }
        [JsonProperty("available")]
        public List<DownloadableNamedUri> Available { get; set; }
    }

    [Serializable]
    public class CatalogImage
    {
        [JsonProperty("remote")]
        public string Remote { get; set; }
        [JsonProperty("local")]
        public string Local { get; set; }
    }
}
