using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace JadeFlix.Domain
{
    [Serializable]
    public class Configuration
    {
        //
        //Disable suggestions, get set needed to deserialize object
        //
        // ReSharper disable once MemberCanBePrivate.Global
        public string FilesCachePath { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public string WwwCachePath { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public string MediaPath { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public string WwwMediaPath { get; set; }
        public IDictionary<string,string> Downloaders { get;set;}
        [JsonIgnore]
        public string VideoFilePattern = "*.mp4|*.mkv|*.avi|*.divx|*.flv";

        private Configuration()
        {
            FilesCachePath = "C:\\tmp";
            WwwCachePath = "/";
            MediaPath = "C:\\tmp";
            WwwMediaPath = "/";
            Downloaders=new Dictionary<string,string>();
        }
        public static Configuration Load()
        {
            var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.OSVersion.Platform == PlatformID.Win32NT ? "config.windows.json" : "config.linux.json");
            var data = File.ReadAllText(configFile);
            return JsonConvert.DeserializeObject<Configuration>(data);
        }
    }
}
