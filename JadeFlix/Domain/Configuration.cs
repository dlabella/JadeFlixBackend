using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeFlix.Domain
{
    [Serializable]
    public class Configuration
    {
        public string FilesCachePath { get; set; } = string.Empty;
        public string WwwCachePath { get; set; } = string.Empty;
        public string MediaPath { get; set; } = string.Empty;
        public string WwwMediaPath { get; set; } = string.Empty;
        [JsonIgnore]
        public string VideoFilePattern = "*.mp4|*.mkv|*.avi|*.divx|*.flv";
        public Configuration()
        {
            FilesCachePath = "C:\\tmp";
            WwwCachePath = "/";
            MediaPath = "C:\\tmp";
            WwwMediaPath = "/";
        }
        public static Configuration Load()
        {
            string configFile = "";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.windows.json");

            }
            else
            {
                configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.linux.json");
            }
            var data = File.ReadAllText(configFile);
            return JsonConvert.DeserializeObject<Configuration>(data);
        }
    }
}
