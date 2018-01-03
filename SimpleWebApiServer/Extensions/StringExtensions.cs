using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleWebApiServer.Extensions
{
    public static class StringExtensions
    {
        public static string FormatJson(this string json, params string[] values)
        {
            var result = json;
            for(int i = 0; i < values.Length; i++)
            {
                result = result.Replace("{" + i + "}", values[i].ToString());
            }
            return result;
        }
    }
}
