using Common;
using Newtonsoft.Json;
using System;
using static System.String;

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

        public string UId => Url?.ToString().EncodeToBase64();

        public int CompareTo(object obj)
        {
            if (obj is NamedUri other)
            {
                return Compare(Name, other.Name, StringComparison.Ordinal);
            }
            else
            {
                return -1;
            }
        }
    }
}
