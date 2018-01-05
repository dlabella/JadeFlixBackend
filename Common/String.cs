using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{
    public static class String
    {
        //public static string CleanDirectoryName(this string toCleanPath, string replaceWith = "-")
        //{
        //    var invalidFileChars = Path.GetInvalidPathChars().ToList();
        //    invalidFileChars.Add('!');
        //    invalidFileChars.Add('/');
        //    invalidFileChars.Add('\\');
        //    //clean bad filename chars  
        //    foreach (char badChar in invalidFileChars)
        //    {
        //        toCleanPath = toCleanPath.Replace(badChar.ToString(), replaceWith);
        //    }
        //    if (string.IsNullOrWhiteSpace(replaceWith) == false)
        //    {
        //        toCleanPath = toCleanPath.Replace(replaceWith.ToString() + replaceWith.ToString(), replaceWith.ToString());
        //    }
        //    return toCleanPath;
        //}

        //public static string CleanDirectoryPath(this string toCleanPath, string replaceWith = "-")
        //{
        //    var invalidPathChars = Path.GetInvalidPathChars().ToList();
        //    invalidPathChars.Add('!');
        //    if (toCleanPath.Length > 2 && toCleanPath[1] == ':')
        //    {
        //        toCleanPath = toCleanPath[0] + ":" + toCleanPath.Substring(2).Replace(":", replaceWith);
        //    }
        //    else
        //    {
        //        toCleanPath = toCleanPath.Replace(":", replaceWith);
        //    }
        //    foreach (char badChar in invalidPathChars)
        //    {
        //        toCleanPath = toCleanPath.Replace(badChar.ToString(), replaceWith);
        //    }
        //    if (string.IsNullOrWhiteSpace(replaceWith) == false)
        //    {
        //        toCleanPath = toCleanPath.Replace(replaceWith.ToString() + replaceWith.ToString(), replaceWith.ToString());
        //    }
        //    return toCleanPath;
        //}

        public static string ToSafeName(this string toCleanPath, string replaceWith = "-")
        {
            var invalidFileChars = Path.GetInvalidFileNameChars().ToList();
            invalidFileChars.Add('!');
            invalidFileChars.Add('/');
            invalidFileChars.Add('\\');
            foreach(var invalidDirChar in Path.GetInvalidPathChars())
            {
                if (!invalidFileChars.Contains(invalidDirChar))
                {
                    invalidFileChars.Add(invalidDirChar);
                }
            }
            //clean bad filename chars  
            foreach (char badChar in invalidFileChars)
            {
                toCleanPath = toCleanPath.Replace(badChar.ToString(), replaceWith);
            }
            if (string.IsNullOrWhiteSpace(replaceWith) == false)
            {
                toCleanPath = toCleanPath.Replace(replaceWith.ToString() + replaceWith.ToString(), replaceWith.ToString());
            }
            return toCleanPath;
        }

        public static string ToSafePath(this string toCleanPath, string replaceWith = "-")
        {
            StringBuilder safePath = new StringBuilder();
            string[] pathParts = toCleanPath.Split(new char[] { PathSeparator });
            int i = 0;
            int lastPart = (pathParts.Length - 1);
            foreach(var part in pathParts)
            {
                if (i==0 && part.EndsWith(":"))
                {
                    safePath.Append(part);
                }
                else
                {
                    safePath.Append(part.ToSafeName());
                }
                
                if (i < lastPart)
                {
                    safePath.Append(PathSeparator);
                }
                i++;
            }
            return safePath.ToString();
        }

        public static string Between(this string src, string from, string to, int startIndex = 0, bool includeStart = false, bool includeEnd = false)
        {
            var start = src.IndexOf(from, startIndex, StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }
            if (!includeStart)
            {
                start += from.Length;
            }
            var end = src.IndexOf(to, start, StringComparison.Ordinal);
            if (end < 0)
            {
                return src.Substring(start);
            }
            if (includeEnd)
            {
                end += to.Length;
            }

            return src.Substring(start, end - start);
        }

        public static string Between(this string src, string from, string to, ref int startIndex, bool includeStart = false, bool includeEnd = false)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            if (startIndex > src.Length)
            {
                startIndex = src.Length;
                return string.Empty;
            }
            var start = src.IndexOf(from, startIndex, StringComparison.Ordinal);
            if (start < 0)
            {
                startIndex = -1;
                return string.Empty;
            }
            if (!includeStart)
            {
                start += from.Length;
            }
            var end = src.IndexOf(to, start, StringComparison.Ordinal);
            if (end < 0)
            {
                startIndex = end;
                return src.Substring(start);
            }
            if (includeEnd)
            {
                end += to.Length;
            }
            startIndex = end;
            return src.Substring(start, end - start);
        }

        public static string EncodeToBase64(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes).Replace("/", "_");
        }

        public static string DecodeFromBase64(this string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData)) return string.Empty;
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData.Replace("_", "/"));
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static char PathSeparator 
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    return '\\';
                }
                return '/';
            }
        }
    }
}
