namespace WebDownloader.Domain.EventHandlers
{
    public delegate void DownloadChangedEventHandler(object sender, DownloadChangedEventArgs e);

    public delegate void DownloadCompletedEventHandler(object sender, DownloadCompletedEventArgs e);
}