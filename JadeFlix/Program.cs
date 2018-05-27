using Common.Logging;
using SimpleWebApiServer;
using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Threading;

namespace JadeFlix
{
    internal static class Program
    {
        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);

        private static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += SigTermEventHandler;
            Console.CancelKeyPress += CancelHandler;

            try
            {
                AppContext.Initialize();
                PrintConfig();
                var port = GetIntArgument(args, 1);
                var ip = GetStringArgument(args, 0);
                var urlPrefix = GetStringArgument(args, 2);
                var debug = GetStringArgument(args, 3);

                if (debug.ToLower().EndsWith("debug"))
                {
                    System.Diagnostics.Trace.Listeners.Add(new ConsoleTraceListener());
                }

                var server = new WebServer(ip, port, urlPrefix);

                RegisterRequestHandlers(server);
                RegisterMediaScrapers();

                server.Run();
                Logger.Debug($"Listening at {ip}:{port} with urlPrefix {urlPrefix}");
                Logger.Debug("Press Ctrl+C to exit ...");
                ResetEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Logger.Exception("General failure exception: " + ex.Message);
            }
        }

        private static void PrintConfig()
        {
            Logger.Debug("** CONFIG **");
            Logger.Debug($"file cache path:    {AppContext.Config.FilesCachePath}");
            Logger.Debug($"www cache path:     {AppContext.Config.WwwCachePath}");
            Logger.Debug($"file media path:    {AppContext.Config.MediaPath}");
            Logger.Debug($"www media path:     {AppContext.Config.WwwMediaPath}");
            Logger.Debug($"Video file pattern: {AppContext.Config.VideoFilePattern}");
            Logger.Debug("************");
        }

        private static string GetStringArgument(IReadOnlyList<string> arguments, int index)
        {
            return arguments.Count > index ? arguments[index] : string.Empty;
        }

        private static int GetIntArgument(IReadOnlyList<string> arguments, int index)
        {
            if (arguments.Count <= index)
            {
                return 0;
            }
            var num = GetStringArgument(arguments, index);
            return int.TryParse(num, out var inum) ? inum : 0;
        }

        private static void RegisterMediaScrapers()
        {
            AppContext.MediaScrapers.Add(new Services.Scrapers.AnimeFlv());
        }

        private static void RegisterRequestHandlers(WebServer server)
        {
            server.RegisterRequestHandler(new Api.GetRecent(WebServer.Cache));
            server.RegisterRequestHandler(new Api.GetItem(WebServer.Cache));
            server.RegisterRequestHandler(new Api.PostItem(WebServer.Cache));
            server.RegisterRequestHandler(new Api.GetMedia(WebServer.Cache));
            server.RegisterRequestHandler(new Api.GetMediaUrl(WebServer.Cache));
            server.RegisterRequestHandler(new Api.GetDownloads(WebServer.Cache));
            server.RegisterRequestHandler(new Api.GetLocal(WebServer.Cache));
            server.RegisterRequestHandler(new Api.FindItem(WebServer.Cache));
            server.RegisterRequestHandler(new Api.Download(WebServer.Cache));
            server.RegisterRequestHandler(new Api.BatchDownload(WebServer.Cache));
            server.RegisterRequestHandler(new Api.Session());
        }

        private static void SigTermEventHandler(AssemblyLoadContext obj)
        {
            ResetEvent.Set();
            Logger.Debug("Unloading...");
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            ResetEvent.Set();
            Logger.Debug("Exiting...");
        }
    }
}
