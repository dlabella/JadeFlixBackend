//using JadeFlix.Domain;
//using Common;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using static JadeFlix.Domain.Enums;
//using Common.Logging;

//namespace JadeFlix.Services.Scrapers
//{
//    public class AnimeYt : MediaScraper
//    {
//        public AnimeYt() : base("AnimeYt", EntryType.Multi, new Uri("http://animeyt.tv"), new TimeSpan(0, 5, 0))
//        {
//        }

//        public override IEnumerable<NamedUri> GetMediaUrls(Uri url)
//        {
//            var cacheKey = this.Name + "-" + url;
//            var contents = string.Empty;

//            contents = GetContents(url);

//            if (string.IsNullOrEmpty(contents)) yield return default(NamedUri);

//            var idx = 0;
//            var mirrors = contents.Between("dr.showIframe =", "};");
//            var loopCount = 0;
//            do
//            {
//                var mirror = mirrors.Between("(mirror == ", "}", ref idx);
//                if (idx >= 0)
//                {
//                    string name = mirror.Between("/*", "*/");
//                    if (!string.IsNullOrEmpty(name) && name.Contains(','))
//                    {
//                        name = name.Substring(name.IndexOf(',') + 1).Trim();
//                    }
//                    string mirrorUrl = mirror.Between("src=\"", "\"");
//                    if (!mirrorUrl.ToLower().Contains("media.php") &&
//                        !mirrorUrl.ToLower().Contains("dailymotion.com"))
//                    {
//                        yield return new NamedUri()
//                        {
//                            Name = name,
//                            Url = new Uri(mirrorUrl)
//                        };
//                    }
//                }
//                loopCount++;
//            } while (idx >= 0 && loopCount < 10);
//        }

//        private Uri TransformUri(Uri uri)
//        {
//            var url = uri.ToString();
//            if (url.Contains("/ver/") &&
//                url.Contains("-sub-espanol"))
//            {
//                //By Episode request
//                var processed = url.Replace("-sub-espanol", "").Replace("/ver", "");
//                processed = processed.Substring(0, processed.LastIndexOf('-'));
//                Trace.TraceInformation("Processed Url: " + uri + " To :" + processed);
//                return new Uri(processed);
//            }
//            else
//            {
//                //By Simple Id request
//                if (!url.ToLower().Contains(BaseUrl.ToString()))
//                {
//                    return new Uri(BaseUrl + "/" + uri);
//                }
//            }
//            return uri;
//        }

//        public override CatalogItem Get(Uri url)
//        {
//            var contents = string.Empty;

//            contents = GetContents(url);

//            var tvshow = new CatalogItem()
//            {
//                ScrapedBy = Name
//            };
//            var idx = 0;
//            var idCut = url.ToString();
//            tvshow.Group = EntryGroup.Anime;
//            tvshow.Id = idCut.Substring(idCut.LastIndexOf("/", StringComparison.Ordinal) + 1);
//            tvshow.Url = url.ToString();
//            tvshow.Banner = contents.Between("class=\"serie_banner__img\" src=\"", "\"", ref idx);
//            var basecut = contents.Between("<main", "/main>", ref idx, true, true);
//            idx = 0;
//            tvshow.Poster = basecut.Between("class=\"serie-header__img\"", "alt").Between("src=\"", "\"");
//            var nameCut = basecut.Between("class=\"serie-header__title\"", "\"", ref idx);
//            tvshow.Name = nameCut.Between(">", "<").Replace("\n", " ").Trim();
//            tvshow.Plot = basecut.Between("id=\"serie-description\">", "</p>").Replace("\n", "").Trim();
//            tvshow.Kind = EntryType.TvShow;
//            var urls = basecut.Between("class=\"serie-capitulos__list\"", "class=\"ed-container\"");
//            var urlIdx = 0;
//            var completed = false;
//            do
//            {
//                var item = urls.Between("class=\"serie-capitulos__list__item\"", "</div>", ref urlIdx);
//                if (urlIdx < 0)
//                {
//                    completed = true;
//                }
//                else
//                {
//                    var epurl = item.Between("href=\"", "\"");
//                    var epName = item.Between("<a", "/a>").Between("\">", "<");
//                    tvshow.Media.Remote.Add(new DownloadableNamedUri
//                    {
//                        Name = epName.Replace(" sub español", "").Replace(" Sub Español", "").Trim(),
//                        Url = new Uri(epurl)
//                    });
//                }

//            } while (!completed);
//            var kindName = "Unknown";
//            switch (tvshow.Kind)
//            {
//                case EntryType.Movie:
//                    kindName = "Movies";
//                    break;
//                case EntryType.Ova:
//                    kindName = "Ovas";
//                    break;
//                case EntryType.TvShow:
//                    kindName = "TvShow";
//                    break;
//            }
//            string entryPath = Path.Combine(AppContext.Config.MediaPath, kindName, tvshow.Id);
//            if (Directory.Exists(entryPath))
//            {
//                var files = Directory.GetFiles(entryPath, AppContext.Config.VideoFilePattern).ToList();
//                files.Sort();
//                foreach (var file in files)
//                {
//                    var episode = new NamedUri
//                    {
//                        Name = file,
//                        Url = new Uri(AppContext.Config.WwwMediaPath + "/" + kindName + "/" + file, UriKind.Relative)
//                    };
//                    tvshow.Media.Local.Add(episode);
//                }
//            }
//            return tvshow;
//        }

//        //public override CatalogItem GetMovie(Uri uri)
//        //{
//        //    var url = TransformUri(uri);

//        //    var contents = string.Empty;

//        //    contents = GetContents(url);

//        //    if (string.IsNullOrEmpty(contents)) return null;

//        //    var movie = new CatalogItem()
//        //    {
//        //        ScrapedBy = Name
//        //    };
//        //    var idx = 0;
//        //    var idCut = url.ToString();
//        //    movie.Group = EntryGroup.Anime;
//        //    movie.Id = idCut.Substring(idCut.LastIndexOf("/", StringComparison.Ordinal) + 1);
//        //    movie.Url = url.ToString();
//        //    movie.Banner = contents.Between("class=\"serie_banner__img\" src=\"", "\"", ref idx);
//        //    var basecut = contents.Between("<main", "/main>", ref idx, true, true);
//        //    idx = 0;
//        //    movie.Poster = basecut.Between("class=\"serie-header__img\"", "alt").Between("src=\"", "\"");
//        //    var nameCut = basecut.Between("class=\"serie-header__title\"", "\"", ref idx);
//        //    movie.Name = nameCut.Between(">", "<").Replace("\n", " ").Trim();
//        //    movie.Plot = basecut.Between("id=\"serie-description\">", "</p>").Replace("\n", "").Trim();
//        //    movie.Kind = EntryType.Movie;
//        //    var urls = basecut.Between("class=\"serie-capitulos__list\"", "class=\"ed-container\"");
//        //    var urlIdx = 0;
//        //    var completed = false;
//        //    do
//        //    {
//        //        var item = urls.Between("class=\"serie-capitulos__list__item\"", "</div>", ref urlIdx);
//        //        if (urlIdx < 0)
//        //        {
//        //            completed = true;
//        //        }
//        //        else
//        //        {
//        //            var epurl = item.Between("href=\"", "\"");
//        //            var epName = item.Between("<a", "/a>").Between("\">", "<");
//        //            movie.Media.Remote.Add(new DownloadableNamedUri()
//        //            {
//        //                Name = epName.Replace(" sub español", "").Replace(" Sub Español", "").Trim(),
//        //                Url = new Uri(epurl)
//        //            });
//        //        }

//        //    } while (!completed);
//        //    return movie;
//        //}

//        public override List<CatalogItem> GetRecent()
//        {
//            Logger.Debug("[" + this.Name + "] Gettinge recent TvShows");

//            List<CatalogItem> entries = new List<CatalogItem>();

//            var contents = string.Empty;

//            contents = GetContents(BaseUrl);

//            if (string.IsNullOrEmpty(contents)) return null;

//            int index = 0;
//            var data = contents.Between("class=\"capitulos-grid ed-container\"", "class=\"ed-container\"");
//            do
//            {
//                var strip = data.Between("class=\"capitulos-grid__item ed-item\"", "</div>", ref index);
//                if (index >= 0 && index <= data.Length)
//                {
//                    var subIdx = 0;
//                    var entry = new CatalogItem();

//                    var subStrip = strip.Between("<a", "</a>", ref subIdx);
//                    var contentLink = subStrip.Between("href=\"", "\"");

//                    var contenturl = contentLink
//                                    .Replace("-sub-espanol", "")
//                                    .Replace("-audio-latino", "");

//                    var s = contenturl.LastIndexOf("/", StringComparison.Ordinal) + 1;
//                    var e = contenturl.LastIndexOf("-", StringComparison.Ordinal) + 1;

//                    int.TryParse(contenturl.Substring(e), out int episodeCount);
//                    entry.Id = episodeCount > 0 ? contenturl.Substring(s, e - s - 1) : contenturl.Substring(s);
//                    entry.Preview = subStrip.Between("src=\"", "\"");

//                    entry.Name = subStrip.Between("alt=\"", "\"")
//                               .Replace("Sub Español", "")
//                               .Replace("Audio Latino", "");

//                    if (entry.Name.Contains(" Película"))
//                    {
//                        var l = contentLink.LastIndexOf("/", StringComparison.Ordinal) + 1;
//                        entry.Url = contentLink.Substring(l);
//                        entry.Name = entry.Name.Substring(0, entry.Name.LastIndexOf("Película")).Trim();
//                        entry.Kind = EntryType.Movie;
//                    }
//                    if (entry.Name.Contains("OVA"))
//                    {
//                        var l = contentLink.LastIndexOf("/", StringComparison.Ordinal) + 1;
//                        entry.Url = contentLink.Substring(l);
//                        entry.Name = entry.Name.Substring(0, entry.Name.LastIndexOf("OVA")).Trim();
//                        entry.Kind = EntryType.Ova;
//                    }
//                    else
//                    {
//                        if (episodeCount > 0)
//                        {
//                            entry.Url = contenturl.Substring(0, contenturl.Length - (episodeCount.ToString().Length + 1)).Replace("/ver/", "/");
//                            entry.Name = entry.Name.Substring(0, entry.Name.LastIndexOf(episodeCount.ToString())).Trim();
//                            entry.Properties.Add(ItemProperty.LastEpisode, episodeCount.ToString());
//                            entry.Kind = EntryType.TvShow;
//                        }
//                        else
//                        {
//                            entry.Url = contenturl;
//                            entry.Kind = EntryType.Movie;
//                        }
//                    }
//                    entry.ScrapedBy = Name;
//                    entry.Group = EntryGroup.Anime;
//                    entries.Add(entry);
//                }
//            } while (index >= 0 && index <= data.Length);
//            return entries;
//        }

//        public override string GetMediaDownloadUrl(Uri url)
//        {
//            var urlStr = url.ToString();
//            if (urlStr.Contains("mp4.php"))
//            {
//                return GetMp4DownloadUrl(url);
//            }

//            return string.Empty;
//        }

//        private string GetMp4DownloadUrl(Uri url)
//        {
//            var data = GetContents(url);
//            var downloadUrl = data.Between("sources: [{file:'", "'");
//            return downloadUrl;
//        }

//        private string GetAnimeYtDownloadUrl(Uri url)
//        {
//            //string json = "{'v_token': 'aUt6c1ZUUGt2NHpPQk5XUkh2SE9zZz09','handler': 'Animeyt', 'token':'eyJjdCI6IlVaMkduc25YQnVpNUI1cUpwVzJtTFE9PSIsIml2IjoiN2Q2MWEzYmUyYjVhMDkyMWZjM2ZhNWRkOTQ0ZjNiOGMiLCJzIjoiMzQyM2U5MDgyYzM0NmU4YSJ9'}";
//            var urlStr = url.ToString();
//            var start = urlStr.IndexOf("id=");
//            var token = urlStr.Substring(start + 3);
//            string req = "http://s.animeyt.tv/v4/gsuite_animeyt.php";
//            var postData = new List<KeyValuePair<string, string>>
//            {
//                new KeyValuePair<string, string>("v_token", token),
//                new KeyValuePair<string, string>("handler", "AnimeYt")
//            };
//            var data = AppContext.Web.PostData(new Uri(req), postData);
//            var downloadUrl = data.Between("\"AnimeYT\":\"", "\"");
//            downloadUrl = System.Text.RegularExpressions.Regex.Unescape(downloadUrl);
//            return downloadUrl;
//        }

//        public override List<CatalogItem> Find(string name)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
