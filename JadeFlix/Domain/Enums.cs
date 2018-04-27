using System;

namespace JadeFlix.Domain
{
    public class Enums
    {
        public enum ItemProperty{
            LastEpisode
        }

        [Flags]
        public enum EntryType
        {
            Unknown,
            TvShow,
            Ova,
            Movie,
            Multi
        }

        [Flags]
        public enum EntryGroup
        {
            Unknown,
            Normal,
            Anime
        }
    }
}
