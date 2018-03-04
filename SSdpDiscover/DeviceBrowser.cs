using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SsdpDiscover
{
    public class DeviceBrowser { 
        public IEnumerable<SsdpResponse> Scan()
        {
            Console.WriteLine("Searching...");
            var responses = new List<SsdpResponse>();

            var localEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var ssdpEndPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            using (var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpSocket.Bind(localEndPoint);
                udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ssdpEndPoint.Address, IPAddress.Any));

                //const string SearchString = "M-SEARCH * HTTP/1.1\r\nHost: 239.255.255.250:1900\r\nMan: \"ssdp:discover\"\r\nST: ssdp:all\r\nMX: 1\r\n\r\n";
                const string SearchString = "M-SEARCH * HTTP/1.1\r\nHost: 239.255.255.250:1900\r\nMan: \"ssdp:discover\"\r\nST: urn:dial-multiscreen-org:service:dial:1\r\nMX: 3\r\n\r\n";

                udpSocket.SendTo(System.Text.Encoding.ASCII.GetBytes(SearchString), ssdpEndPoint);

                var buffer = new byte[4096];
                var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                while (timeout > DateTime.UtcNow)
                {
                    if (udpSocket.Available > 0)
                    {
                        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                        var size = udpSocket.ReceiveFrom(buffer, ref remote);
                        var response = SsdpResponse.Parse(System.Text.Encoding.ASCII.GetString(buffer, 0, size), remote);
                        if (response != null)
                        {
                            yield return response;
                        }
                    }
                }
            }
           
        }
    }
}
