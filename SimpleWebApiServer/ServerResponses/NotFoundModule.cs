using System.Net;

namespace SimpleWebApiServer.RequestResponses
{
    public class NotFoundResponse:ServerResponse
    {
        string _data;
        public NotFoundResponse():base(404)
        {
            _data = _data = "{\"ServerError\":{\"Code\":404,\"Exception\":\"{0}\"}}";
        }

        public override string GetResponse(HttpListenerRequest request, RequestParameters parameters)
        {
            return string.Format(_data,request.Url.AbsolutePath);
        }
    }
}
