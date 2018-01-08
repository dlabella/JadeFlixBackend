using JadeFlix.Domain;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static JadeFlix.Domain.Enums;
using Common.Logging;

namespace JadeFlix.Services.Scrapers
{
    public class AnimeFlv : MediaScraper
    {
        
        public AnimeFlv() : base("AnimeFlv", EntryType.Multi, new Uri("http://animeflv.net/"), new TimeSpan(0, 10, 0))
        {
        }
        public override List<CatalogItem> GetRecent()
        {
            Logger.Debug("[" + this.Name + "] Gettinge recent TvShows");

            List<CatalogItem> entries = new List<CatalogItem>();

            var contents = string.Empty;

            contents = GetContents(BaseUrl);
            var items = contents.Between("ListEpisodios AX", "</ul>");
            var catalogItems = ParseListItems(items).Where(x=> x!=null && !string.IsNullOrEmpty(x.Name)).ToList();
            return catalogItems;
        }
        public override CatalogItem GetTvShow(Uri url)
        {
            var entry = new CatalogItem();
            var content = GetContents(url);
            entry.Kind = EntryType.TvShow;
            entry.Url = url.ToString();
            entry.Group = EntryGroup.Anime;
            entry.Banner = ConcatToBaseUrl(content.Between("class=\"Bg\"", ">").Between("background-image:url(", ")"));
            entry.Poster = ConcatToBaseUrl(content.Between("class=\"AnimeCover\"", "</figure>").Between("<img src=\"", "\""));
            entry.Name = content.Between("<h1 class=\"Title\">", "<");
            entry.Plot = content.Between("<div class=\"Description\">", "</div>").Between("<p>", "</p>");
            entry.ScrapedBy = this.Name;

            var episodeList = content.Between("<ul class=\"ListCaps\"", "</ul>");
            if (string.IsNullOrWhiteSpace(episodeList))
            {
                episodeList = content.Between("<ul class=\"ListEpisodes\"", "</ul>");
            }
            var idx = 0;
            string episode = string.Empty;
            var episodes = new Dictionary<int, DownloadableNamedUri>();
            do
            {
                episode = episodeList.Between("<li", "</li>", ref idx);
                var media = GetTvShowEpisode(episode);

                if (media.Value != null && !episodes.ContainsKey(media.Key))
                {
                    episodes.Add(media.Key, new DownloadableNamedUri() { Url = media.Value.Url, Name = media.Value.Name });
                }
            } while (idx!=-1);
            entry.Media.Remote = episodes.OrderByDescending(x => x.Key).Select(y => y.Value).ToList();

            DigestCatalogItem(entry);

            return entry;
        }

        private void DigestCatalogItem(CatalogItem item)
        {
            item.Poster = AppContext.LocalScraper.GetOrAddItemPoster(item, item.Poster);
            item.Banner = AppContext.LocalScraper.GetOrAddItemBanner(item, item.Banner);
            item.Preview = AppContext.LocalScraper.GetOrAddItemBanner(item, item.Preview);
            var local = AppContext.LocalScraper.Get(item.GroupName, item.KindName, item.Name);
            if (local != null)
            {
                item.Watching = local.Watching;
            }
        }

        public override CatalogItem GetMovie(Uri url)
        {
            throw new NotImplementedException();
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
                var startIdx = media.Url.ToString().LastIndexOf("-");
                if (startIdx > 0)
                {
                    startIdx++;
                    int.TryParse(media.Url.ToString().Substring(startIdx), out episodeIdx);
                }
            }
            
            return new KeyValuePair<int, NamedUri>(episodeIdx, media);
        }

        public override IEnumerable<NamedUri> GetMediaUrls(Uri url)
        {
            var content = GetContents(url);
            var options = content.Between("var video = [];", "$(document)");
            int videoIdx = 0;
            var handledMedia = GetHandledMediaUrls();
            do
            {
                var videoUrl = string.Empty;
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
                    yield return new NamedUri()
                    {
                        Name = videoUrl.Between("//", "/"),
                        Url = new Uri(videoUrl)
                    };
                }
            } while (videoIdx != -1);
        }

        public override string GetMediaDownloadUrl(Uri url)
        {
            var urlStr = url.ToString();
            var contents = GetContents(url);
            if (urlStr.Contains("efire.php"))
            {
                var realUrl = GetEFireMediaUrl(contents);
                if (!string.IsNullOrEmpty(realUrl))
                {
                    var hoster = System.Text.RegularExpressions.Regex.Unescape(realUrl);
                    if (hoster.Contains("mediafire.com"))
                    {
                        return GetMediaFireUrl(hoster);
                    }
                }
            }
            else if (urlStr.Contains("rapidvideo.com"))
            {
                return GetRapidVideoUrl(contents);
            }
            else if (urlStr.Contains("streamango.com"))
            {
                return GetRapidVideoUrl(contents);
            }
            else if (urlStr.Contains("embed.php"))
            {
                var checkUrl = urlStr.Replace("embed.php", "check.php");
                var key = checkUrl.Between("key=", "&");
                checkUrl = checkUrl.Replace(key, "");
                var data = GetContents(new Uri(checkUrl));
                var realUrl = data.Between("\"file\":\"", "\"");
                return realUrl;
            }
            return string.Empty;
        }

        private List<string> GetHandledMediaUrls()
        {
            return new List<string>() { "efire.php", "rapidvideo.com", "streamango.com", "embed.php" };
        }

        private string GetMediaFireUrl(string url)
        {
            var content = GetContents(new Uri(url));
            var data = content.Between("href='http://download", "'");
            return "http://download"+data;
        }

        private string GetEFireMediaUrl(string content)
        {
            var url = content.Between("$.get('", "'");
            return url;
        }
        private string GetRapidVideoUrl(string content)
        {
            var contentVideo = content.Between("og:url\" content=\"","\"");
            var videoHtml = GetContents(new Uri(contentVideo));
            var videoUrl = videoHtml.Between("<video", "</video>").Between("<source src=\"","\"");
            return videoUrl;
        }

        private string GetStreamAndGoVideoUrl(string content)
        {
            var contentVideo = content.Between("og:url\" content=\"", "\"");
            var videoHtml = GetContents(new Uri(contentVideo));
            var videoUrl = videoHtml.Between("<video", "</video>").Between("<source src=\"", "\"");
            return videoUrl;
        }

        private IEnumerable<CatalogItem> ParseListItems(string contents)
        {
            if (string.IsNullOrEmpty(contents)) yield return null;

            int idx = 0;
            string item;
            do
            {
                item = contents.Between("<li>", "</li>", ref idx);
                var catalogItem = ParseListItem(item);
                yield return catalogItem;
            }
            while (item != string.Empty);
        }

        private CatalogItem ParseListItem(string content)
        {
            bool digest = true;
            if (string.IsNullOrEmpty(content)) return null;
            CatalogItem item = new CatalogItem();
            item.ScrapedBy = this.Name;

            string url = content.Between("<a href=\"", "\"").Replace("/ver/", "/anime/");
            
            
            item.Group = EntryGroup.Anime;
            item.Banner = ConcatToBaseUrl(content.Between("<img src=\"", "\""));
            item.Preview = item.Banner;
            item.Kind = GetItemType(content);
            item.Name = content.Between("Title\">", "<");
            item.Poster = AppContext.LocalScraper.GetItemPoster(item, item.Poster);

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

            if (digest)
            {
                DigestCatalogItem(item);
            }
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

        public override List<CatalogItem> FindTvShow(string name)
        {
            var url = ConcatToBaseUrl($"browse?q=*{Uri.EscapeUriString(name)}*");
            var contents = GetContents(new Uri(url));
            var items = contents.Between("ListAnimes AX", "</ul>");
            var catalogItems = ParseListItems(items).Where(x => x != null && !string.IsNullOrEmpty(x.Name)).ToList();
            return catalogItems;
        }
    }
}
