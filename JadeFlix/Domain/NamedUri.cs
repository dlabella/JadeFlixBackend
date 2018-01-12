using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain
{
    [Serializable]
    public class NamedUri:IComparable
    {
        public NamedUri()
        {

        }
        public NamedUri(string name, Uri url)
        {
            Name = name;
            Url = url;
        }
        [JsonProperty("name")]

        public string Name { get; set; }
        [JsonProperty("url")]

        public Uri Url { get; set; }
        [JsonProperty("uId")]

        public string UId { get { return Url?.ToString().EncodeToBase64(); } }

        public int CompareTo(object obj)
        {
            var other = obj as NamedUri;
            if (other != null)
            {
                return this.Name.CompareTo(other.Name);
            }
            else
            {
                return -1;
            }
        }
    }
}
