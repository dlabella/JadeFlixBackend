using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain
{
    public class SessionResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
