using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeFlix.Services.Scrapers.Fixes
{
    public class RemoveNonParseableJsons
    {
        public static void Apply(DirectoryInfo dir)
        {
            FileInfo data = new FileInfo(Path.Combine(dir.FullName, "data.json"));
            FileInfo tvshow = new FileInfo(Path.Combine(dir.FullName, "tvshow.json"));
            FileInfo info = new FileInfo(Path.Combine(dir.FullName, "info.json"));
            if (data.Exists && tvshow.Exists)
            {
                Logger.Debug("*** Fix RemoveNonParseableJsons, applied on " + dir.FullName);
                data.Delete();
            }
        }
    }
}
