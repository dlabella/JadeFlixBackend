using Newtonsoft.Json;

namespace JadeFlix.Domain
{
    public class SessionResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
