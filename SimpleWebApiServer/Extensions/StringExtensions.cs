namespace SimpleWebApiServer.Extensions
{
    public static class StringExtensions
    {
        public static string FormatJson(this string json, params string[] values)
        {
            var result = json;
            for(var i = 0; i < values.Length; i++)
            {
                result = result.Replace("{" + i + "}", values[i]);
            }
            return result;
        }
    }
}
