using JadeFlix.Domain;
using System;
using System.Collections.Generic;

namespace JadeFlix.Services
{
    public class MediaScrapers
    {
        private readonly Dictionary<string, MediaScraper> _scrapers;
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
            return _scrapers.ContainsKey(name) ? _scrapers[name] : null;
        }
    }
}
