using System.Net;

namespace SimpleWebApiServer
{
    public abstract class ServerResponse
    {
        protected ServerResponse(int code)
        {
            Code = code;
        }
        public int Code { get; }
        public abstract string GetResponse(HttpListenerRequest request, RequestParameters parameters);
    }
}
