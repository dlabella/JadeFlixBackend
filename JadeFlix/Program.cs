using Common.Logging;
using MediaCatalog;
using SimpleWebApiServer;
using System;
using System.Diagnostics;
using System.Runtime.Loader;
using System.Threading;

namespace JadeFlix
{
    class Program
    {
        private static ManualResetEvent resetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += SigTermEventHandler;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);

            try
            {
                AppContext.Initialize();
                int port = GetIntArgument(args,1);
                string ip = GetStringArgument(args,0);
                string urlPrefix = GetStringArgument(args,2);
                string debug = GetStringArgument(args,3);

                if (debug.ToLower().EndsWith("debug"))
                {
                    System.Diagnostics.Trace.Listeners.Add(new ConsoleTraceListener());
                }

                WebServer server = new WebServer(ip, port, urlPrefix);

                RegisterRequestHandlers(server);
                RegisterMediaScrapers();
                
                server.Run();
                Logger.Debug($"Listening at {ip}:{port} with urlPrefix {urlPrefix}");
                Logger.Debug("Press Ctrl+C to exit ...");
                resetEvent.WaitOne();
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
            //MediaScrapers.Add(new AnimeCatalog.Services.Scrapers.AnimeYt(Cache));
            AppContext.MediaScrapers.Add(new JadeFlix.Services.Scrapers.AnimeFlv());
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
        }

        private static void SigTermEventHandler(AssemblyLoadContext obj)
        {
            resetEvent.Set();
            Logger.Debug("Unloading...");
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            resetEvent.Set();
            Logger.Debug("Exiting...");
        }
    }
}
