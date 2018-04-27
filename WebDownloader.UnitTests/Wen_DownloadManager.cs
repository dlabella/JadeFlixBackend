using JadeFlix.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebDownloader.UnitTests
{
    [TestClass]
    public class WhenDownloadManager
    {
        [TestMethod]
        public void EnqueueMultipleDownloads()
        {
            var sut = new DownloadManager();
            sut.Enqueue("a", @"c:\tmp\testa.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null,true);
            sut.Enqueue("b", @"c:\tmp\testb.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null, true);
            sut.Enqueue("c", @"c:\tmp\testc.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null, true);
            sut.Enqueue("d", @"c:\tmp\testd.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null, true);
            sut.Enqueue("e", @"c:\tmp\teste.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null, true);
            sut.Enqueue("f", @"c:\tmp\testf.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null, true);
            sut.Enqueue("g", @"c:\tmp\testg.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null, true);
            sut.Enqueue("h", @"c:\tmp\testh.txt", new Uri("http://www.html5videoplayer.net/videos/toystory.mp4"), null, true);
        }
    }
}
