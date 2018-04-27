using System;

namespace JadeFlix.Domain
{
    public class CacheEntry<T>
    {
        public DateTime CatchUntil { get; set; }
        public T Data { get; set; }
    }
}
