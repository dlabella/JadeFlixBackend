using JadeFlix.Domain;
using JadeFlix.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix
{
    public static class AppContext
    {
        public static MediaScrapers MediaScrapers;
        public static Web Web;
        public static DownloadManager FileDownloader;
        public static Configuration Config;
        public static JadeFlix.Services.Scrapers.LocalScraper LocalScraper;
        public static void Initialize()
        {
            Config = Configuration.Load();
            MediaScrapers = new MediaScrapers();
            Web = new Web();
            FileDownloader = new DownloadManager();
            LocalScraper = new Services.Scrapers.LocalScraper();
            //MediaScrapers.Add(new AnimeCatalog.Services.Scrapers.AnimeYt(Cache));
            MediaScrapers.Add(new JadeFlix.Services.Scrapers.AnimeFlv());
        }
    }
}
