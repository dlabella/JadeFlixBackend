using System;
using System.Globalization;

namespace WebDownloader.Services
{
    public static class ConvertString
    {
        public static decimal ToDecimal(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()))
            {
                return 0;
            }
            return decimal.TryParse(str.Trim(), out decimal val) ? val : 0;
        }

        public static TimeSpan ToTimeSpan(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()))
            {
                return TimeSpan.Zero;
            }
            var values = str.Split(":", 3, StringSplitOptions.RemoveEmptyEntries);
            return values.Length != 3 ? TimeSpan.Zero : new TimeSpan(ToInt(values[0]), ToInt(values[1]), ToInt(values[2]));
        }

        public static int ToInt(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()))
            {
                return 0;
            }
            return int.TryParse(str.Trim(), out int val) ? val : 0;
        }
        public static int ToByteSize(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()))
            {
                return 0;
            }
            decimal val;
            int i;
            var strBs = str.Trim().ToUpper();
            if (strBs.Contains("G"))
            {
                i = strBs.IndexOf("G", StringComparison.Ordinal);
                return decimal.TryParse(strBs.Substring(0, i), NumberStyles.Number, new CultureInfo("en-US"), out val) ? (int)(val * 1073741824) : 0;
            }
            if (strBs.Contains("M"))
            {
                i = strBs.IndexOf("M", StringComparison.Ordinal);
                return decimal.TryParse(strBs.Substring(0, i), NumberStyles.Number, new CultureInfo("en-US"), out val) ? (int)(val * 1048576) : 0;
            }
            if (strBs.Contains("K"))
            {
                i = strBs.IndexOf("K", StringComparison.Ordinal);
                return decimal.TryParse(strBs.Substring(0, i), NumberStyles.Number, new CultureInfo("en-US"), out val) ? (int)(val * 1024) : 0;
            }
            return decimal.TryParse(strBs, out val) ? (int)val : 0;
        }
    }
}
