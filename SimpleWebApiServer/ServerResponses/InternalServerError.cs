using System.Net;
using SimpleWebApiServer.Extensions;

namespace SimpleWebApiServer.ServerResponses
{
    public class InternalServerError:ServerResponse
    {
        public InternalServerError() : base(500)
        {
        }

        public override string GetResponse(HttpListenerRequest request, RequestParameters parameters)
        {
            return "Internal server error";
        }
    }
}
