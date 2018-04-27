using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebDownloader.Domain;
using WebDownloader.Downloaders;

namespace WebDownloader.UnitTests
{
    [TestClass]
    public class WhenDownloaderLineInfo
    {
        [TestMethod]
        public void CurlLineInfoRead()
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

        [TestMethod]
        public void YtLineInfoRead()
        {
            var sut = new YtDownloadInfoParser();
            DownloadInfo data;
            string line;

            line = "[download] 16.8% 374.64KiB ar 133.11KiB/s ETA 00:02";
            data = sut.Parse(line);
            Assert.IsTrue(data.BytesTotal == 383631);
            Assert.IsTrue(data.BytesReceived == 64450);

            line = "[download] 77.8% 374.64KiB ar 133.11KiB/s ETA 00:02";
            data = sut.Parse(line);
            Assert.IsTrue(data.BytesTotal == 383631);
            Assert.IsTrue(data.BytesReceived == 298464);

            line = "[download]   9.7% of 190.38MiB at  1.38MiB/s ETA 02:04";
            data = sut.Parse(line);
            Assert.IsTrue(data.BytesTotal == 199627898);
            Assert.IsTrue(data.BytesReceived == 19363906);

        }
    }
}
