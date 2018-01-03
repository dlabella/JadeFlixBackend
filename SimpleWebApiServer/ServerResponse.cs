using System.Net;

namespace SimpleWebApiServer
{
    public abstract class ServerResponse
    {
        public ServerResponse(int code)
        {
            Code = code;
        }
        public int Code { get; }
        public abstract string GetResponse(HttpListenerRequest request, RequestParameters parameters);
    }
}
