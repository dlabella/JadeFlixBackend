using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain
{
    public class CacheEntry<T>
    {
        public DateTime CatchUntil { get; set; }
        public T Data { get; set; }
    }
}
