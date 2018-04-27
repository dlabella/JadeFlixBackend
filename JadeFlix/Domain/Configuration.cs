using Newtonsoft.Json;
using System;
using System.IO;

namespace JadeFlix.Domain
{
    [Serializable]
    public class Configuration
    {
        public string FilesCachePath { get; }
        public string WwwCachePath { get; }
        public string MediaPath { get; }
        public string WwwMediaPath { get; }
        [JsonIgnore]
        public string VideoFilePattern = "*.mp4|*.mkv|*.avi|*.divx|*.flv";

        private Configuration()
        {
            FilesCachePath = "C:\\tmp";
            WwwCachePath = "/";
            MediaPath = "C:\\tmp";
            WwwMediaPath = "/";
        }
        public static Configuration Load()
        {
            var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.OSVersion.Platform == PlatformID.Win32NT ? "config.windows.json" : "config.linux.json");
            var data = File.ReadAllText(configFile);
            return JsonConvert.DeserializeObject<Configuration>(data);
        }
    }
}
