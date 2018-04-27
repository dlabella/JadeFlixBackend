using Common;
using JadeFlix.Domain;
using System.IO;

namespace JadeFlix.Extensions
{
    public static class CatalogItemExtensions
    {
        public static string GetMediaPath(this CatalogItem item)
        {
            return Path.Combine(AppContext.Config.MediaPath, item.GroupName, item.KindName.ToSafeName(), item.Name.ToSafeName()).ToSafePath();
        }
        public static string GetMediaPath(this CatalogItem item, string fileName)
        {
            return Path.Combine(GetMediaPath(item),fileName.ToSafeName()).ToSafePath();
        }
    }
}
