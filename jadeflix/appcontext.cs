using JadeFlix.Domain;
using JadeFlix.Services;
using JadeFlix.Services.Scrapers;

namespace JadeFlix
{
    public static class AppContext
    {
        public static MediaScrapers MediaScrapers;
        public static Web Web;
        public static DownloadManager FileDownloader;
        public static Configuration Config;
        public static LocalScraper LocalScraper;
        public static void Initialize()
        {
            Config = Configuration.Load();
            MediaScrapers = new MediaScrapers();
            Web = new Web();
            FileDownloader = new DownloadManager();
            LocalScraper = new Services.Scrapers.LocalScraper();
        }
    }
}
