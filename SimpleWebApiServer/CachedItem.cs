using System;

namespace SimpleWebApiServer
{
    public class CachedItem
    {
        public CachedItem() : this(0, string.Empty, string.Empty) { }
        public CachedItem(int id, string source, string value) : this(id, source, value, DateTime.MaxValue) { }
        public CachedItem(int id, string source, string value, DateTime expiration)
        {
            Id = id;
            Source = source;
            Value = value;
            Expiration = expiration;
        }
        public int Id { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }
        public DateTime Expiration { get; set; }
    }
}
