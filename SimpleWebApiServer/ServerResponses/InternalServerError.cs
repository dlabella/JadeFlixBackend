using System.Net;
using SimpleWebApiServer.Extensions;

namespace SimpleWebApiServer.RequestResponses
{
    public class InternalServerError:ServerResponse
    {
        string _data;
        public InternalServerError() : base(500)
        {
            _data = "{\"ServerError\":{\"Code\":500,\"Exception\":\"{0}\"}}";
        }

        public override string GetResponse(HttpListenerRequest request, RequestParameters parameters)
        {
            return StringExtensions.FormatJson(_data,parameters.QueryParameters["&Exception"]);
        }
    }
}
