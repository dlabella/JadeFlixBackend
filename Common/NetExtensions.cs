using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class NetExtensions
    {
        public static async Task<string> GetResponseStringAsync(this WebResponse response)
        {
            string data;
            using (var stm = response.GetResponseStream())
            {
                data = await stm.ReadToEndAsync();
            }
            return data;
        }
    }
}
