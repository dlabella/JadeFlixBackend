using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using static JadeFlix.Domain.Enums;

namespace JadeFlix.Domain
{
    [Serializable]
    public class CatalogItem
    {
        public CatalogItem()
        {
            Properties = new Dictionary<ItemProperty, string>();
            Media = new MediaSource();
        }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("uId")]
        public string UId { get; internal set; }
        [JsonProperty("nId")]
        public string NId { get; internal set; }
        [JsonProperty("plot")]
        public string Plot { get; set; }
        [JsonProperty("url")]
        public string Url {
            get
            {
                return UId?.DecodeFromBase64();
            }
            set
            {
                UId = value?.EncodeToBase64();
            }
        }
        [JsonProperty("banner")]
        public string Banner { get; set; }
        [JsonProperty("poster")]
        public string Poster { get; set; }
        [JsonProperty("preview")]
        public string Preview { get; set; }
        [JsonProperty("name")]
        public string Name
        {
            get
            {
                return NId?.DecodeFromBase64();
            }
            set
            {
                NId = value?.EncodeToBase64();
            }
        }
        [JsonProperty("kind")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EntryType Kind { get; set; } = EntryType.Unknown;
        [JsonProperty("kindName")]
        public string KindName { get { return Kind.ToString(); } }
        [JsonProperty("group")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EntryGroup Group { get; set; } = EntryGroup.Unknown;
        [JsonProperty("groupName")]
        public string GroupName { get { return Group.ToString(); } }
        [JsonProperty("scrapedBy")]
        public string ScrapedBy { get; set; }
        [JsonProperty("media")]
        public MediaSource Media { get; set; }
        [JsonProperty("properties")]
        public Dictionary<ItemProperty, string> Properties{get;set;}
        [JsonProperty("watching")]
        public bool Watching { get; set; }
    }
}
