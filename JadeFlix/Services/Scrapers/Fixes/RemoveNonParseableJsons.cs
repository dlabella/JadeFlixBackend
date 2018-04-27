using Common.Logging;
using System.IO;

namespace JadeFlix.Services.Scrapers.Fixes
{
    public class RemoveNonParseableJsons
    {
        public static void Apply(DirectoryInfo dir)
        {
            FileInfo data = new FileInfo(Path.Combine(dir.FullName, "data.json"));
            FileInfo tvshow = new FileInfo(Path.Combine(dir.FullName, "tvshow.json"));
            if (data.Exists && tvshow.Exists)
            {
                Logger.Debug("*** Fix RemoveNonParseableJsons, applied on " + dir.FullName);
                data.Delete();
            }
        }
    }
}
