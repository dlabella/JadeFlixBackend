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
        private readonly Regex _regexBrackets = new Regex("(\\[.*\\])|(\\(.*\\))");

        private static readonly List<string> MediaExtensions = new List<string>(){
            ".AVI", ".MP4", ".DIVX", ".WMV",
        };

        public static async Task SaveAsync(CatalogItem entry)
        {
            try
            {
                var path = GetDataFile(entry.GroupName, entry.KindName, entry.Name);
                var finfo = new FileInfo(path);
                if (finfo.Directory != null && !finfo.Directory.Exists)
                {
                    Directory.CreateDirectory(finfo.Directory.FullName);
                }
                var tvshowJson = JsonConvert.SerializeObject(entry);
                await File.WriteAllTextAsync(path, tvshowJson);
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while save catalog item data, ex:" + ex.Message, ex);
            }
        }

        public static int Compare(CatalogItem a, CatalogItem b)
        {
            var cmp = a.Kind.CompareTo(b.Kind);
            if (cmp != 0)
            {
                return cmp;
            }

            if (a.UId == null ||
                a.Plot == null) return -1;

            cmp = string.Compare(a.NId, b.NId, StringComparison.Ordinal);
            if (cmp != 0) return cmp;

            cmp = string.Compare(a.UId, b.UId, StringComparison.Ordinal);
            if (cmp != 0) return cmp;

            cmp = string.Compare(a.Plot, b.Plot, StringComparison.Ordinal);
            if (cmp != 0) return cmp;

            if (cmp != 0)
            {
                return cmp;
            }
            foreach (var item in a.Media.Remote)
            {
                if (b.Media.Remote.All(x => x.UId != item.UId))
                {
                    return -1;
                }
            }
            foreach (var item in b.Media.Remote)
            {
                if (a.Media.Remote.All(x => x.UId != item.UId))
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

        private static CatalogItem GetFastAsync(string groupName, string kindName, string name)
        {
            var item = BuidlCatalogItem(groupName, kindName, name);

            if (string.IsNullOrEmpty(item.ScrapedBy))
            {
                item.ScrapedBy = "Local";
            }

            SetLocalImages(item);

            return item;
        }

        public async Task<List<CatalogItem>> GetItemsAsync(string group, string kind)
        {
            var items = new List<CatalogItem>();
            var path = Path.Combine(AppContext.Config.MediaPath, group, kind);
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) return items;

            foreach (var directory in dir.GetDirectories().OrderByDescending(x => x.CreationTime).ThenBy(x=>x.Name))
            {
                Logger.Debug("Loading: " + directory.Name);
                var item = GetFastAsync(group, kind, directory.Name);
                items.Add(item);
            }

            await Task.Delay(10);
            return items; 
        }

        private static async Task<CatalogItem> Load(string groupName, string kindName, string name)
        {
            var data = GetDataFile(groupName, kindName, name);
            if (!File.Exists(data))
            {
                return BuidlCatalogItem(groupName, kindName, name);
            }

            CatalogItem item;

            try
            {
                var jsonData = await File.ReadAllTextAsync(data);
                if (jsonData != null)
                {
                    item = await Task.Run(() => JsonConvert.DeserializeObject<CatalogItem>(jsonData));

                    if (string.IsNullOrEmpty(item.Name) &&
                        string.IsNullOrEmpty(item.KindName) &&
                        string.IsNullOrEmpty(item.GroupName))
                    {
                        item = BuidlCatalogItem(groupName, kindName, name);
                    }
                }
                else
                {
                    item = BuidlCatalogItem(groupName, kindName, name);
                }
            }
            catch (Exception)
            {
                item = BuidlCatalogItem(groupName, kindName, name);
            }

            return item;
        }

        private static CatalogItem BuidlCatalogItem(string groupName, string kindName, string name)
        {
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

        private static string GetDataFile(string groupName, string kindName, string name)
        {
            var path = GetMediaPath(groupName, kindName, name);
            var data = Path.Combine(path, "data.json");
            return data;
        }
        public static string GetOrAddItemBanner(CatalogItem item, string url)
        {
            return GetOrAddItemImage("banner", item.GroupName, item.KindName, item.Name, url);
        }

        public static string GetOrAddItemPoster(CatalogItem item, string url)
        {
            return GetOrAddItemImage("poster", item.GroupName, item.KindName, item.Name, url);
        }

        public static string GetItemPoster(CatalogItem item, string url)
        {
            var img = GetItemImage("poster", item.GroupName, item.KindName, item.Name);
            return !string.IsNullOrEmpty(img) ? img : url;
        }

        private static string GetOrAddItemImage(string imageType, string group, string kind, string name, string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            var path = Path.Combine(AppContext.Config.MediaPath, group, kind, name.ToSafeName(), imageType + ".jpg");
            path = path.ToSafePath();
            var fileName = imageType + ".jpg";
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

        private static void DownloadFileIfNotLocal(string url, string path, bool diasbleTracking=false)
        {
            if (!url.Contains(AppContext.Config.WwwCachePath) &&
                    !url.Contains(AppContext.Config.WwwMediaPath))
            {
                AppContext.FileDownloader.Enqueue(url, path, new Uri(url),disableTracking: diasbleTracking);
            }
        }

        private static string GetItemImage(string imageType, string group, string kind, string name)
        {
            var path = Path.Combine(AppContext.Config.MediaPath, group, kind, name.ToSafeName(), imageType + ".jpg");
            path = path.ToSafePath();
            var fileName = imageType + ".jpg";
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
                var finfo = new FileInfo(file);
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

        private static bool IsInvalidFilename(string path, string fileName)
        {
            var file = fileName;
            if (fileName.Contains(path))
            {
                var fileIndex = path.Length;
                fileIndex++;
                file = fileName.Substring(fileIndex);
            }

            return FileContainsInvalidChars(file);
        }

        private static bool FileContainsInvalidChars(string file)
        {
            return Path.GetInvalidFileNameChars().Any(invalidChar => file.Any(x => x == invalidChar));
        }
        private int ExtractNumberFromMediaFile(string text)
        {
            var sanitizedName = _regexBrackets.Replace(text, "").ToLower().Replace(".mp4", "");
            var number = string.Join("", sanitizedName.Where(char.IsDigit));
            return !string.IsNullOrEmpty(number) ? int.Parse(number) : 0;
        }
        public static void SaveImagesToLocal(CatalogItem item)
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

        public static void SetLocalImages(CatalogItem item)
        {
            var wwwpath = GetWwwPath(item);
            item.Banner = wwwpath + "/banner.jpg";
            item.Poster = wwwpath + "/poster.jpg";
        }

        private static async Task SetLocalPlot(CatalogItem item)
        {
            var path = GetMediaPath(item);
            var plotFile = Path.Combine(path, "plot.txt");
            if (File.Exists(plotFile))
            {
                item.Plot = await File.ReadAllTextAsync(plotFile);
            }
        }

        private static string GetWwwPath(CatalogItem item)
        {
            var path = AppContext.Config.WwwMediaPath.TrimEnd('/') + "/" + item.GroupName + "/" + item.KindName + "/" + item.Name.ToSafeName();
            return path;
        }

        private static string GetMediaPath(CatalogItem item)
        {
            return GetMediaPath(item.GroupName, item.KindName, item.Name);
        }

        private static string GetMediaPath(string group, string kind, string name)
        {
            var path = Path.Combine(AppContext.Config.MediaPath, group, kind, name.ToSafeName());
            return path.ToSafePath();
        }

    }
}
