using System.Net;
using SimpleWebApiServer.Extensions;

namespace SimpleWebApiServer.ServerResponses
{
    public class InternalServerError:ServerResponse
    {
        private readonly string _data;
        public InternalServerError() : base(500)
        {
            _data = "{\"ServerError\":{\"Code\":500,\"Exception\":\"{0}\"}}";
        }

        public override string GetResponse(HttpListenerRequest request, RequestParameters parameters)
        {
            return _data.FormatJson(parameters.QueryParameters["&Exception"]);
        }
    }
}
