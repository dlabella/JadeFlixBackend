using Common;
using JadeFlix.Domain;
using System;
using System.Collections.Generic;
using System.Text;

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
