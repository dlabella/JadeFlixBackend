using JadeFlix.Domain;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static JadeFlix.Domain.Enums;
using Common.Logging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JadeFlix.Services.Scrapers
{
    public class LocalScraper
    {
        //"(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
        private Regex regexBrackets = new Regex("(\\[.*\\])|(\\(.*\\))");
        public LocalScraper()
        {
        }

        private static List<string> MediaExtensions = new List<string>(){
            ".AVI", ".MP4", ".DIVX", ".WMV",
        };
        public async Task SaveAsync(CatalogItem entry)
        {
            try
            {
                var path = GetDataFile(entry.GroupName, entry.KindName, entry.Name);
                var finfo = new FileInfo(path);
                if (!finfo.Directory.Exists)
                {
                    Directory.CreateDirectory(finfo.Directory.FullName);
                }
                var tvshowJson = JsonConvert.SerializeObject(entry);
                await File.WriteAllTextAsync(path, tvshowJson);
            }
            catch (Exception ex)
            {
                Logger.Debug("Error while save catalog item data, ex:" + ex.Message);
            }
        }

        public int Compare(CatalogItem a, CatalogItem b)
        {
            int cmp = 0;

            cmp = a.Kind.CompareTo(b.Kind);
            if (cmp != 0)
            {
                return cmp;
            }

            if (a.UId == null ||
                a.Plot == null) return -1;

            cmp = a.NId.CompareTo(b.NId);
            if (cmp != 0) return cmp;

            cmp = a.UId.CompareTo(b.UId);
            if (cmp != 0) return cmp;

            cmp = a.Plot.CompareTo(b.Plot);
            if (cmp != 0) return cmp;

            if (cmp != 0)
            {
                return cmp;
            }
            foreach (var item in a.Media.Remote)
            {
                if (!b.Media.Remote.Any(x => x.UId == item.UId))
                {
                    return -1;
                }
            }
            foreach (var item in b.Media.Remote)
            {
                if (!a.Media.Remote.Any(x => x.UId == item.UId))
                {
                    return 1;
                }
            }

            return 0;
        }

        public async Task<CatalogItem> GetAsync(string groupName, string kindName, string name)
        {
            var item = await Load(groupName, kindName, name);

            if (string.IsNullOrEmpty(item.ScrapedBy))
            {
                item.ScrapedBy = "Local";
            }

            await SetLocalPlot(item);

            SetLocalImages(item);

            SetLocalMedia(item);

            return item;
        }

        public async Task<List<CatalogItem>> GetItemsAsync(string group, string kind)
        {
            var items = new List<CatalogItem>();
            var path = Path.Combine(AppContext.Config.MediaPath, group, kind);
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) return items;

            foreach (var directory in dir.GetDirectories().OrderBy(x => x.Name))
            {
                ProcessFixes(directory);

                Logger.Debug("Loading: " + directory.Name);
                var item = await GetAsync(group, kind, directory.Name);
                if (item != null && (item.Media.Local.Count > 0 || item.Watching))
                {
                    items.Add(item);
                }
            }
            return items.OrderByDescending(x=>x.Watching).ToList();
        }

        private void ProcessFixes(DirectoryInfo dir)
        {
            Fixes.FixOlderCovers.Apply(dir);
            Fixes.RemoveNonParseableJsons.Apply(dir);
        }

        public async Task<CatalogItem> Load(string groupName, string kindName, string name)
        {
            var data = GetDataFile(groupName, kindName, name);
            if (File.Exists(data))
            {
                try
                {
                    var jsonData = await File.ReadAllTextAsync(data);
                    if (jsonData != null)
                    {
                        var item = JsonConvert.DeserializeObject<CatalogItem>(jsonData);
                        if (!string.IsNullOrEmpty(item.Name) &&
                            !string.IsNullOrEmpty(item.KindName) &&
                            !string.IsNullOrEmpty(item.GroupName))
                        {
                            return item;
                        }
                    }
                }
                catch (Exception) { }
            }
            var group = Enum.Parse<EntryGroup>(groupName);
            var kind = Enum.Parse<EntryType>(kindName);
            var newItem = new CatalogItem()
            {
                Group = group,
                Name = name,
                Kind = kind
            };
            return newItem;
        }

        private string GetDataFileAsync(CatalogItem item)
        {
            return GetDataFile(item.GroupName, item.KindName, item.Name);
        }

        private string GetDataFile(string groupName, string kindName, string name)
        {
            var path = GetMediaPath(groupName, kindName, name);
            var data = Path.Combine(path, "data.json");
            return data;
        }
        public string GetOrAddItemBanner(CatalogItem item, string url)
        {
            return GetOrAddItemImage("banner", item.GroupName, item.KindName, item.Name, url);
        }
        public string GetOrAddItemBanner(string group, string kind, string name, string url)
        {
            return GetOrAddItemImage("banner", group, kind, name, url);
        }
        public string GetOrAddItemPoster(CatalogItem item, string url)
        {
            return GetOrAddItemImage("poster", item.GroupName, item.KindName, item.Name, url);
        }
        public string GetOrAddItemPoster(string group, string kind, string name, string url)
        {
            return GetOrAddItemImage("poster", group, kind, name, url);
        }
        public string GetItemPoster(CatalogItem item, string url)
        {
            return GetItemImage("poster", item.GroupName, item.KindName, item.Name);
        }
        public string GetItemPoster(string group, string kind, string name)
        {
            return GetItemImage("poster", group, kind, name);
        }
        public string GetOrAddItemPreview(CatalogItem item, string url)
        {
            return GetOrAddItemImage("preview", item.GroupName, item.KindName, item.Name, url);
        }
        public string GetOrAddItemPreview(string group, string kind, string name, string url)
        {
            return GetOrAddItemImage("preview", group, kind, name, url);
        }
        private string GetOrAddItemImage(string imageType, string group, string kind, string name, string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            string fileName = string.Empty;

            var path = Path.Combine(AppContext.Config.MediaPath, group, kind, name.ToSafeName(), imageType + ".jpg");
            path = path.ToSafePath();
            fileName = imageType + ".jpg";
            if (File.Exists(path))
            {
                return AppContext.Config.WwwMediaPath + "/" + group + "/" + kind + "/" + name.ToSafeName() + "/" + fileName.ToSafeName();
            }
            else if (imageType == "poster")
            {
                DownloadFileIfNotLocal(url, path, true);
            }

            fileName = name + "_" + imageType + ".jpg";
            var cache = Path.Combine(AppContext.Config.FilesCachePath, group, kind, fileName.ToSafeName());
            cache = cache.ToSafePath();

            if (File.Exists(cache))
            {
                return AppContext.Config.WwwCachePath + "/" + group + "/" + kind + "/" + fileName.ToSafeName();
            }
            else
            {
                DownloadFileIfNotLocal(url, cache, true);
                return url;
            }
        }

        private void DownloadFileIfNotLocal(string url, string path, bool diasbleTracking=false)
        {
            if (!url.Contains(AppContext.Config.WwwCachePath) &&
                    !url.Contains(AppContext.Config.WwwMediaPath))
            {
                AppContext.FileDownloader.Enqueue(url, path, new Uri(url),disableTracking: diasbleTracking);
            }
        }

        private string GetItemImage(string imageType, string group, string kind, string name)
        {
            string fileName = string.Empty;

            var path = Path.Combine(AppContext.Config.MediaPath, group, kind, name.ToSafeName(), imageType + ".jpg");
            path = path.ToSafePath();
            fileName = imageType + ".jpg";
            if (File.Exists(path))
            {
                return AppContext.Config.WwwMediaPath + "/" + group + "/" + kind + "/" + name.ToSafeName() + "/" + fileName.ToSafeName();
            }

            fileName = name + "_" + imageType + ".jpg";
            var cache = Path.Combine(AppContext.Config.FilesCachePath, group, kind, fileName.ToSafeName());
            cache = cache.ToSafePath();

            if (File.Exists(cache))
            {
                return AppContext.Config.WwwCachePath + "/" + group + "/" + kind + "/" + fileName.ToSafeName();
            }
            return string.Empty;
        }

        public void SetLocalMedia(CatalogItem item)
        {
            var path = GetMediaPath(item);
            var wwwpath = GetWwwPath(item);

            if (!Directory.Exists(path)) return;
            var files = Directory.GetFiles(path);
            var local = new List<NamedUri>(files.Length);

            foreach (var file in files)
            {
                if (IsInvalidFilename(path, file))
                {
                    Logger.Debug($"*** {file} has invalid filename and cannot be included");
                    continue;
                }
                FileInfo finfo = new FileInfo(file);
                var extension = finfo.Extension.ToUpperInvariant();
                if (MediaExtensions.Contains(extension))
                {
                    local.Add(new NamedUri()
                    {
                        Name = finfo.Name,
                        Url = new Uri(wwwpath + "/" + Uri.EscapeUriString(finfo.Name).Replace(":", "%3A"), UriKind.RelativeOrAbsolute)
                    });
                }
            }
            item.Media.Local = local.OrderByDescending(x => ExtractNumberFromMediaFile(x.Name)).ToList();

        }

        public bool IsInvalidFilename(string path, string fileName)
        {
            int fileIndex = 0;
            if (fileName.Contains(path))
            {
                fileIndex = path.Length;
                fileIndex++;
            }
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (fileName.Substring(fileIndex).Any(x => x == invalidChar))
                {
                    return true;
                }
            }
            return false;
        }
        private int ExtractNumberFromMediaFile(string text)
        {
            var sanitizedName = regexBrackets.Replace(text, "").ToLower().Replace(".mp4", "");
            var number = string.Join("", sanitizedName.Where(Char.IsDigit));
            if (!string.IsNullOrEmpty(number))
            {
                return int.Parse(number);
            }
            return 0;
        }
        public void SaveImagesToLocal(CatalogItem item)
        {
            var path = GetMediaPath(item);
            var poster = Path.Combine(path, "poster.jpg");
            var banner = Path.Combine(path, "banner.jpg");

            if (!File.Exists(poster) && Uri.IsWellFormedUriString(item.Poster, UriKind.Absolute))
            {
                AppContext.FileDownloader.Enqueue(item.Poster, poster, new Uri(item.Poster), disableTracking: true);
            }

            if (!File.Exists(banner) && Uri.IsWellFormedUriString(item.Banner, UriKind.Absolute))
            {
                AppContext.FileDownloader.Enqueue(item.Banner, banner, new Uri(item.Banner), disableTracking: true);
            }
        }

        public void SetLocalImages(CatalogItem item)
        {
            var wwwpath = GetWwwPath(item);
            item.Banner = wwwpath + "/banner.jpg";
            item.Poster = wwwpath + "/poster.jpg";
        }

        public async Task SetLocalPlot(CatalogItem item)
        {
            var path = GetMediaPath(item);
            var plotFile = Path.Combine(path, "plot.txt");
            if (File.Exists(plotFile))
            {
                item.Plot = await File.ReadAllTextAsync(plotFile);
            }
        }

        public string GetWwwPath(CatalogItem item)
        {
            var path = AppContext.Config.WwwMediaPath.TrimEnd('/') + "/" + item.GroupName + "/" + item.KindName + "/" + item.Name.ToSafeName();
            return path;
        }

        public string GetMediaPath(CatalogItem item)
        {
            return GetMediaPath(item.GroupName, item.KindName, item.Name);
        }

        public string GetMediaPath(string group, string kind, string name)
        {
            var path = Path.Combine(AppContext.Config.MediaPath, group, kind, name.ToSafeName());
            return path.ToSafePath();
        }

    }
}
