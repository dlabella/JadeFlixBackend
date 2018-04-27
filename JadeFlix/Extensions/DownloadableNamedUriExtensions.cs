using Common;
using JadeFlix.Domain;

namespace JadeFlix.Extensions
{
    public static class DownloadableNamedUriExtensions
    {
        public static string GetFileName(this DownloadableNamedUri downloadableUri, string extension=".mp4")
        {
            return (downloadableUri.Name + extension).ToSafeName();
        }
    }
}
