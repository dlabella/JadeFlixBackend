using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain.ApiParameters
{
    public class BatchApiParams:ApiParamtersBase
    {
        public string Group { get; set; }
        public string Kind { get; set; }
        public string Url { get; set; }
        public MediaScraper Scraper { get; set; }
        public string Key
        {
            get { return Scraper + Group + Kind + Url; }
        }
        public override bool AreValid
        {
            get
            {
                if (string.IsNullOrEmpty(Group) ||
                    string.IsNullOrEmpty(Kind) ||
                    string.IsNullOrEmpty(Url) ||
                    Scraper == null)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
