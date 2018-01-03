using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeFlix.Services.Scrapers.Fixes
{
    public class FixOlderCovers
    {
        public static void Apply(DirectoryInfo dir)
        {
            bool moved = false;
            bool applied = false;
            try
            {
                FileInfo cover = new FileInfo(Path.Combine(dir.FullName, "cover.jpg"));
                FileInfo banner = new FileInfo(Path.Combine(dir.FullName, "banner.jpg"));
                FileInfo poster = new FileInfo(Path.Combine(dir.FullName, "poster.jpg"));

                if (banner.Exists && !poster.Exists)
                {
                    moved = true;
                    banner.MoveTo(poster.FullName);
                    applied = true;
                }
                else if (banner.Exists && poster.Exists)
                {
                    banner.Delete();
                    applied = true;
                }
                if (cover.Exists && !poster.Exists && !moved)
                {
                    cover.MoveTo(poster.FullName);
                    applied = true;
                }
                if (applied)
                {
                    Logger.Debug("*** Fix FixOlderCovers, applied on " + dir.FullName);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"Can't apply fixes, because {ex.Message}");
            }
        }
    }
}
