using Common.Logging;
using SimpleWebApiServer;
using System;
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
                var port = GetIntArgument(args,1);
                var ip = GetStringArgument(args,0);
                var urlPrefix = GetStringArgument(args,2);
                var debug = GetStringArgument(args,3);

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
                Logger.Debug("General failure exception: " + ex.Message);
            }
        }

        private static string GetStringArgument(string[] arguments, int index)
        {
            if (arguments.Length > index)
            {
                return arguments[index];
            }
            return string.Empty;
        }

        private static int GetIntArgument(string[] arguments, int index)
        {
            if (arguments.Length > index)
            {
                var num = GetStringArgument(arguments, index);
                if (int.TryParse(num, out int inum))
                {
                    return inum;
                }
            }
            return 0;
        }

        private static void RegisterMediaScrapers()
        {
            AppContext.MediaScrapers.Add(new Services.Scrapers.AnimeFlv());
        }

        private static void RegisterRequestHandlers(WebServer server)
        {
            server.RegisterRequestHandler(new Api.GetRecent(server.Cache));
            server.RegisterRequestHandler(new Api.GetItem(server.Cache));
            server.RegisterRequestHandler(new Api.PostItem(server.Cache));
            server.RegisterRequestHandler(new Api.GetMedia(server.Cache));
            server.RegisterRequestHandler(new Api.GetMediaUrl(server.Cache));
            server.RegisterRequestHandler(new Api.GetDownloads(server.Cache));
            server.RegisterRequestHandler(new Api.GetLocal(server.Cache));
            server.RegisterRequestHandler(new Api.FindItem(server.Cache));
            server.RegisterRequestHandler(new Api.Download(server.Cache));
            server.RegisterRequestHandler(new Api.BatchDownload(server.Cache));
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
