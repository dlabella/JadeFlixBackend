﻿using Common.Logging;
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
                int port = int.Parse(args[1]);
                string ip = args[0];
                string urlPrefix = args[2];

                WebServer server = new WebServer(ip, port, urlPrefix);
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

                System.Diagnostics.Trace.Listeners.Add(new ConsoleTraceListener());

                server.Run();
                Logger.Debug($"Listening at {ip}:{port} with urlPrefix {urlPrefix}");
                Logger.Debug("Press Ctrl+C to exit ...");
                resetEvent.WaitOne();
            }
            catch(Exception ex)
            {
                Logger.Debug("General failure exception: " + ex.Message);
            }
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
