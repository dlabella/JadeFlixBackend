using SsdpDiscover;
using System;
using System.Linq;
using Tmds.MDns;

namespace GoogleCast
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var deviceBrowser = new DeviceBrowser();
            foreach (var response in deviceBrowser.Scan())
            {
                var endPoint = response.RemoteEndPoint.ToString().Trim();
                if (!endPoint.StartsWith("1.1.1.2:") &&
                    !endPoint.StartsWith("192.168.1.1:"))
                {
                    Console.WriteLine($"Headers: {String.Join(", ", response.Headers)}");
                    Console.WriteLine($"StatusCode: {response.StatusCode}");
                    Console.WriteLine($"Reason: {response.ReasonPhrase}");
                    Console.WriteLine($"Address: {response.RemoteEndPoint.Address.ToString()}");
                }
            }
            Console.WriteLine($"Done");
            Console.ReadKey();
        }
    }
}
