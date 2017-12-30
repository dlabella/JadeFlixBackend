using JadeFlix.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Services
{
    public class MediaScrapers
    {
        private Dictionary<string, MediaScraper> _scrapers;
        public MediaScrapers()
        {
            _scrapers = new Dictionary<string, MediaScraper>(StringComparer.OrdinalIgnoreCase);
        }
        public void Add(MediaScraper scraper)
        {
            _scrapers.Add(scraper.Name, scraper);
        }
        public MediaScraper Get(string name)
        {
            if (_scrapers.ContainsKey(name))
            {
                return _scrapers[name];
            }
            else
            {
                return null;
            }
        }
    }
}
