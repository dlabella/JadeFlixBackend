using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class StreamExtensions
    {
        public static async Task<string> ReadToEndAsync(this Stream src, int bufferLength = 4096)
        {
            int bytesReaded;
            if (!src.CanRead) return string.Empty;
            var buffer = new byte[bufferLength];
            var data = new List<byte>();
            do
            {
                bytesReaded = await src.ReadAsync(buffer, 0, bufferLength);
                if (bytesReaded <= 0) continue;
                data.AddRange(bytesReaded != bufferLength ? buffer : buffer.Take(bytesReaded));
            }
            while (bytesReaded > 0);
            return Encoding.UTF8.GetString(data.ToArray());
        }
    }
}
