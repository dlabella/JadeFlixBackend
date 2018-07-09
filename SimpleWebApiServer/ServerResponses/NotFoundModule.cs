using System.Net;

namespace SimpleWebApiServer.ServerResponses
{
    public class NotFoundResponse:ServerResponse
    {
        public NotFoundResponse():base(404)
        {
        }

        public override string GetResponse(HttpListenerRequest request, RequestParameters parameters)
        {
            return $"Path {request.Url.AbsolutePath} not found";
        }
    }
}
