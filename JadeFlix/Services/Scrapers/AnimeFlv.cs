using JadeFlix.Domain;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static JadeFlix.Domain.Enums;
using Common.Logging;
using System.Threading.Tasks;

namespace JadeFlix.Services.Scrapers
{
    public class AnimeFlv : MediaScraper
    {

        public AnimeFlv() : base("AnimeFlv", EntryType.Multi, new Uri("https://animeflv.net/"), new TimeSpan(0, 10, 0))
        {
        }
        public override async Task<List<CatalogItem>> GetRecentAsync()
        {
            Logger.Debug("[" + Name + "] Gettinge recent TvShows");

            var contents = await GetContentsAsync(BaseUrl);
            var items = contents.Between("ListEpisodios AX", "</ul>");
            var catalogItems = (await ParseListItems(items)).Where(x => x != null && !string.IsNullOrEmpty(x.Name)).ToList();
            return catalogItems;
        }
        public override async Task<CatalogItem> GetAsync(Uri url)
        {
            var entry = new CatalogItem();
            var content = await GetContentsAsync(url);
            entry.Kind = EntryType.TvShow;
            entry.Url = url.ToString();
            entry.Group = EntryGroup.Anime;
            entry.Banner = ConcatToBaseUrl(content.Between("class=\"Bg\"", ">").Between("background-image:url(", ")"));
            entry.Poster = ConcatToBaseUrl(content.Between("class=\"AnimeCover\"", "</figure>").Between("<img src=\"", "\""));
            entry.Name = content.Between("<h2 class=\"Title\">", "<");
            entry.Plot = content.Between("<div class=\"Description\">", "</div>").Between("<p>", "</p>");
            entry.ScrapedBy = Name;

            var episodes = GetEpisodes(content);
            entry.Media.Remote = episodes.OrderByDescending(x => x.Key).Select(y => y.Value).ToList();

            await DigestCatalogItem(entry);

            return entry;
        }

        private Dictionary<int, DownloadableNamedUri> GetEpisodes(string content)
        {
            //var anime_info = ["2932", "Hinamatsuri", "hinamatsuri"];
            //var episodes = [[12, 49416],[11, 49330],[10, 49261],[9, 49194],[8, 49126],[7, 49060],[6, 48995],[5, 48926],[4, 48859],[3, 48794],[2, 48734],[1, 48674]];
            //var last_seen = 0;
            var episodeItems = new Dictionary<int, DownloadableNamedUri>();
            var data = content.Between("var anime_info", "var last_seen");
            var baseData = data.Between("[", "];").Replace(" ", string.Empty).Replace("\"", string.Empty).Split(",");
            var episodes = data.Between("episodes = [", "];").Replace(" ", string.Empty);
            foreach (var episodePair in episodes.Split("],"))
            {
                var episodeData = episodePair.Replace("[", string.Empty).Replace("]", string.Empty).Split(',');
                int.TryParse(episodeData[0], out var index);
                var url = $"/ver/{episodeData[1]}/{baseData[2]}-{episodeData[0]}";
                var uri = new Uri(ConcatToBaseUrl(url));
                episodeItems.Add(index, new DownloadableNamedUri { Name = $"{baseData[1]} {episodeData[0]}", Url = uri });
            }

            return episodeItems;
        }

        private static async Task DigestCatalogItem(CatalogItem item)
        {
            item.Poster = LocalScraper.GetOrAddItemPoster(item, item.Poster);
            item.Banner = LocalScraper.GetOrAddItemBanner(item, item.Banner);
            item.Preview = LocalScraper.GetOrAddItemBanner(item, item.Preview);
            var local = await AppContext.LocalScraper.GetAsync(item.GroupName, item.KindName, item.Name);
            if (local != null)
            {
                item.Watching = local.Watching;
            }
        }

        private KeyValuePair<int, NamedUri> GetTvShowEpisode(string content)
        {
            if (string.IsNullOrEmpty(content)) return default(KeyValuePair<int, NamedUri>);

            var media = new NamedUri();
            var episodeIdx = -1;
            var url = content.Between("<a href=\"", "\"");

            if (string.IsNullOrEmpty(url) || !url.StartsWith('/')) return default(KeyValuePair<int, NamedUri>);

            media.Url = new Uri(ConcatToBaseUrl(url));
            media.Name = content.Between("<h3 class=\"Title\">", "<");
            if (string.IsNullOrWhiteSpace(media.Name))
            {
                media.Name = content.Between("<a", "/a>").Between(">", "<");
            }
            if (content.Contains("<p>Episodio"))
            {
                int.TryParse(content.Between("<p>Episodio", "</p>").Trim(), out episodeIdx);
                media.Name = media.Name + (episodeIdx > -1 ? " " + episodeIdx.ToString() : "");
            }
            else
            {
                var startIdx = media.Url.ToString().LastIndexOf("-", StringComparison.Ordinal);
                if (startIdx <= 0)
                {
                    return new KeyValuePair<int, NamedUri>(episodeIdx, media);
                }
                startIdx++;
                int.TryParse(media.Url.ToString().Substring(startIdx), out episodeIdx);
            }

            return new KeyValuePair<int, NamedUri>(episodeIdx, media);
        }

        public override async Task<IEnumerable<NamedUri>> GetMediaUrlsAsync(Uri url)
        {
            var content = await GetContentsAsync(url);
            var options = content.Between("var video = [];", "$(document)");
            var videoIdx = 0;
            var handledMedia = GetHandledMediaUrls();
            var mediaUrls = new List<NamedUri>();
            do
            {
                string videoUrl;
                if (videoIdx == 0 && options.Contains("window.open(\""))
                {
                    videoUrl = options.Between("window.open(\"", "\"", ref videoIdx);
                }
                else
                {
                    videoUrl = options.Between("<iframe", "<", ref videoIdx).Between("src=\"", "\"");
                }
                if (!string.IsNullOrEmpty(videoUrl) && handledMedia.Any(x => videoUrl.Contains(x)))
                {
                    mediaUrls.Add(new NamedUri()
                    {
                        Name = videoUrl.Between("//", "/"),
                        Url = new Uri(videoUrl)
                    });

                }
                else if (videoUrl != null && videoUrl.Contains("redirector.php"))
                {
                    var redirectorUrl = ParseRedirectorUrl(videoUrl);
                    if (redirectorUrl != string.Empty)
                    {
                        mediaUrls.Add(new NamedUri
                        {
                            Name = redirectorUrl.Between("//", "/"),
                            Url = new Uri(redirectorUrl)
                        });
                    }
                }
            } while (videoIdx != -1);
            return mediaUrls;
        }

        private static string ParseRedirectorUrl(string url)
        {
            var server = url.Between("server=", "&").ToLower();
            var value = url.Between("value=", "\"");
            switch (server)
            {
                case "rv":
                    return string.Format("https://www.rapidvideo.com/e/{0}&q=720p", value);
                //case "mega":
                //    return string.Format("https://mega.nz/embed#{0}", value);
                //case "mango":
                //    return string.Format("https://streamango.com/embed/{0}", value);
                //case "openload":
                //    return string.Format("https://streamango.com/embed/{0}", value);
                default:
                    return string.Empty;
            }
        }

        public override async Task<string> GetMediaDownloadUrlAsync(Uri url)
        {
            var urlStr = url.ToString();
            var contents = await GetContentsAsync(url);
            if (urlStr.Contains("efire.php"))
            {
                var realUrl = GetEFireMediaUrl(contents);
                if (string.IsNullOrEmpty(realUrl))
                {
                    return string.Empty;
                }
                var hoster = System.Text.RegularExpressions.Regex.Unescape(realUrl);
                if (hoster.Contains("mediafire.com"))
                {
                    return await GetMediaFireUrlAsync(hoster);
                }
            }
            else if (urlStr.Contains("rapidvideo.com"))
            {
                return await GetRapidVideoUrlAsync(contents);
            }
            else if (urlStr.Contains("streamango.com"))
            {
                return await GetRapidVideoUrlAsync(contents);
            }
            else if (urlStr.Contains("embed.php"))
            {
                var checkUrl = urlStr.Replace("embed.php", "check.php");
                var key = checkUrl.Between("key=", "&");
                checkUrl = checkUrl.Replace(key, "");
                var data = await GetContentsAsync(new Uri(checkUrl));
                var realUrl = data.Between("\"file\":\"", "\"");
                return realUrl;
            }
            return string.Empty;
        }

        private List<string> GetHandledMediaUrls()
        {
            return new List<string>() { "efire.php", "rapidvideo.com", "streamango.com", "embed.php" };
        }

        private async Task<string> GetMediaFireUrlAsync(string url)
        {
            var content = await GetContentsAsync(new Uri(url));
            var data = content.Between("'http://download", "'");
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            return "http://download" + data;
        }

        private string GetEFireMediaUrl(string content)
        {
            var url = content.Between("$.get('", "'");
            return url;
        }
        private async Task<string> GetRapidVideoUrlAsync(string content)
        {
            var contentVideo = content.Between("og:url\" content=\"", "\"");
            var videoHtml = await GetContentsAsync(new Uri(contentVideo));
            var videoUrl = videoHtml.Between("<video", "</video>").Between("<source src=\"", "\"");
            return videoUrl;
        }

        private async Task<IEnumerable<CatalogItem>> ParseListItems(string contents)
        {
            if (string.IsNullOrEmpty(contents)) return null;
            var catalogItems = new List<CatalogItem>();
            int idx = 0;
            string item;
            do
            {
                item = contents.Between("<li>", "</li>", ref idx);
                var catalogItem = await ParseListItem(item);
                catalogItems.Add(catalogItem);
            }
            while (item != string.Empty);
            return catalogItems;
        }

        private async Task<CatalogItem> ParseListItem(string content)
        {
            if (string.IsNullOrEmpty(content)) return null;
            var item = new CatalogItem { ScrapedBy = Name };

            var url = content.Between("<a href=\"", "\"").Replace("/ver/", "/anime/");


            item.Group = EntryGroup.Anime;
            item.Banner = ConcatToBaseUrl(content.Between("<img src=\"", "\""));
            item.Preview = item.Banner;
            item.Kind = GetItemType(content);
            item.Name = content.Between("Title\">", "<");
            item.Poster = LocalScraper.GetItemPoster(item, item.Banner);

            FillCustomProperties(item, content);

            var start = "/anime/".Length;
            if (ItemHasEpisodes(item) && item.Kind == EntryType.TvShow)
            {
                item.Url = ConcatToBaseUrl(url.Substring(0, url.LastIndexOf('-')));

                var end = url.LastIndexOf('-');
                item.Id = url.Substring(start, end - start);
            }
            else
            {
                item.Url = ConcatToBaseUrl(url);
                item.Id = url.Substring(start);
            }

            await DigestCatalogItem(item);

            return item;
        }
        private bool ItemHasEpisodes(CatalogItem item)
        {
            return item.Properties.ContainsKey(ItemProperty.LastEpisode) &&
                !string.IsNullOrEmpty(item.Properties[ItemProperty.LastEpisode]) &&
                item.Properties[ItemProperty.LastEpisode] != "0";
        }
        private void FillCustomProperties(CatalogItem item, string content)
        {
            switch (item.Kind)
            {
                case EntryType.TvShow:
                    FillTvShowCustomProperties(item, content);
                    break;
            }
        }

        private void FillTvShowCustomProperties(CatalogItem item, string content)
        {
            if (int.TryParse(content.Between("Capi\">Episodio", "<").Trim(), out int episodeNumber))
            {
                item.Properties[ItemProperty.LastEpisode] = episodeNumber.ToString();
            }
        }

        private EntryType GetItemType(string content)
        {
            if (content.Contains("Capi\">Episodio") || content.Contains("\"Type tv\">Anime"))
            {
                return EntryType.TvShow;
            }
            if (content.Contains("\"Type movie\">"))
            {
                return EntryType.Movie;
            }
            return EntryType.TvShow;
        }

        public override async Task<List<CatalogItem>> FindAsync(string name)
        {
            var url = ConcatToBaseUrl($"browse?q=*{Uri.EscapeUriString(name)}*");
            var contents = await GetContentsAsync(new Uri(url));
            var items = contents.Between("ListAnimes AX", "</ul>");
            var catalogItems = (await ParseListItems(items)).Where(x => x != null && !string.IsNullOrEmpty(x.Name)).ToList();
            return catalogItems;
        }
    }
}
