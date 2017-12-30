using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebDownloader.Domain;
using WebDownloader.Downloaders;

namespace WebDownloader.UnitTests
{
    [TestClass]
    public class When_DownloaderLineInfo
    {
        [TestMethod]
        public void LineInfoRead()
        {
            var sut = new CurlDownloadInfoParser(0);
            DownloadInfo data;
            string line;

            line = "100  296M  100  296M    0     0   796k      0  0:06:20  0:06:20 --:--:-- 743k";
            data = sut.Parse(line);
            Assert.IsTrue(data.BytesTotal== 310378496);
            Assert.IsTrue(data.BytesReceived == 310378496);

            line = "1  296M  1  500k    0     0   796k      0  0:06:20  0:06:20 --:--:-- 743k";
            data = sut.Parse(line);
            Assert.IsTrue(data.BytesTotal== 310378496);
            Assert.IsTrue(data.BytesReceived == 512000);
        }
    }
}
