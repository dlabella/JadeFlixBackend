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
        
        public AnimeFlv() : base("AnimeFlv", EntryType.Multi, new Uri("http://animeflv.net/"), new TimeSpan(0, 10, 0))
        {
        }
        public override async Task<List<CatalogItem>> GetRecentAsync()
        {
            Logger.Debug("[" + Name + "] Gettinge recent TvShows");

            var contents = await GetContentsAsync(BaseUrl);
            var items = contents.Between("ListEpisodios AX", "</ul>");
            var catalogItems = (await ParseListItems(items)).Where( x=> x!=null && !string.IsNullOrEmpty(x.Name)).ToList();
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

            var episodeList = content.Between("<ul class=\"ListCaps\"", "</ul>");
            if (string.IsNullOrWhiteSpace(episodeList))
            {
                episodeList = content.Between("<ul class=\"ListEpisodes\"", "</ul>");
            }
            var idx = 0;
            var episodes = new Dictionary<int, DownloadableNamedUri>();
            do
            {
                var episode = episodeList.Between("<li", "</li>", ref idx);
                var media = GetTvShowEpisode(episode);

                if (media.Value != null && !episodes.ContainsKey(media.Key))
                {
                    episodes.Add(media.Key, new DownloadableNamedUri() { Url = media.Value.Url, Name = media.Value.Name });
                }
            } while (idx!=-1);
            entry.Media.Remote = episodes.OrderByDescending(x => x.Key).Select(y => y.Value).ToList();

            await DigestCatalogItem(entry);

            return entry;
        }

        private async Task DigestCatalogItem(CatalogItem item)
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
            int episodeIdx = -1;
            var url = content.Between("<a href=\"", "\"");

            if (string.IsNullOrEmpty(url) || !url.StartsWith('/')) return default(KeyValuePair<int, NamedUri>);

            media.Url = new Uri(ConcatToBaseUrl(url));
            media.Name = content.Between("<h3 class=\"Title\">", "<");
            if (string.IsNullOrWhiteSpace(media.Name))
            {
                media.Name = content.Between("<a","/a>").Between(">", "<");
            }
            if (content.Contains("<p>Episodio"))
            {
                int.TryParse(content.Between("<p>Episodio", "</p>").Trim(), out episodeIdx);
                media.Name = media.Name + (episodeIdx > -1 ? " " + episodeIdx.ToString() : "");
            }
            else
            {
                var startIdx = media.Url.ToString().LastIndexOf("-", StringComparison.Ordinal);
                if (startIdx > 0)
                {
                    startIdx++;
                    int.TryParse(media.Url.ToString().Substring(startIdx), out episodeIdx);
                }
            }
            
            return new KeyValuePair<int, NamedUri>(episodeIdx, media);
        }

        public override async Task<IEnumerable<NamedUri>> GetMediaUrlsAsync(Uri url)
        {
            var content = await GetContentsAsync(url);
            var options = content.Between("var video = [];", "$(document)");
            int videoIdx = 0;
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
                    videoUrl = options.Between("<iframe","<", ref videoIdx).Between("src=\"", "\"");
                }
                if (!string.IsNullOrEmpty(videoUrl) && handledMedia.Any(x=>videoUrl.Contains(x)))
                {
                    mediaUrls.Add(new NamedUri()
                    {
                        Name = videoUrl.Between("//", "/"),
                        Url = new Uri(videoUrl)
                    });
                }
            } while (videoIdx != -1);
            return mediaUrls;
        }

        public override async Task<string> GetMediaDownloadUrlAsync(Uri url)
        {
            var urlStr = url.ToString();
            var contents = await GetContentsAsync(url);
            if (urlStr.Contains("efire.php"))
            {
                var realUrl = GetEFireMediaUrl(contents);
                if (!string.IsNullOrEmpty(realUrl))
                {
                    var hoster = System.Text.RegularExpressions.Regex.Unescape(realUrl);
                    if (hoster.Contains("mediafire.com"))
                    {
                        return await GetMediaFireUrlAsync(hoster);
                    }
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
            return "http://download"+data;
        }

        private string GetEFireMediaUrl(string content)
        {
            var url = content.Between("$.get('", "'");
            return url;
        }
        private async Task<string> GetRapidVideoUrlAsync(string content)
        {
            var contentVideo = content.Between("og:url\" content=\"","\"");
            var videoHtml = await GetContentsAsync(new Uri(contentVideo));
            var videoUrl = videoHtml.Between("<video", "</video>").Between("<source src=\"","\"");
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
            var item = new CatalogItem {ScrapedBy = Name};

            var url = content.Between("<a href=\"", "\"").Replace("/ver/", "/anime/");
            
            
            item.Group = EntryGroup.Anime;
            item.Banner = ConcatToBaseUrl(content.Between("<img src=\"", "\""));
            item.Preview = item.Banner;
            item.Kind = GetItemType(content);
            item.Name = content.Between("Title\">", "<");
            item.Poster = LocalScraper.GetItemPoster(item, item.Poster);

            FillCustomProperties(item, content);

            var start = "/anime/".Length;
            if (ItemHasEpisodes(item) && item.Kind==EntryType.TvShow)
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
