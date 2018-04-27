using Common.Logging;
using System;
using System.Diagnostics;

namespace WebDownloader.Services
{
    public static class Sys
    {
        public static void RunProcess(
           string command,
           string arguments,
           bool redirectErrorToStd = true,
           Action<string> onReadLine = null,
           Action<string> onErrorLine = null,
           Action<int> onExit = null)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            Logger.Debug($"Command Exceuted: {command} {arguments}");
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null) return;

                onReadLine?.Invoke(e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null) return;

                if (onErrorLine != null && !redirectErrorToStd)
                {
                    onErrorLine(e.Data);
                }
                if (onReadLine != null && redirectErrorToStd)
                {
                    onReadLine(e.Data);
                }
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            onExit?.Invoke(process.ExitCode);
        }
    }
}
