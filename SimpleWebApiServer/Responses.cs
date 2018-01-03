using SimpleWebApiServer.RequestResponses;
using System;
using System.Net;

namespace SimpleWebApiServer
{
    internal static class Responses
    {
        private static NotFoundResponse _notFound = new NotFoundResponse();
        private static InternalServerError internalServerError = new InternalServerError();
        public static string NotFound(HttpListenerRequest req, RequestParameters parameters)
        {
            return _notFound.GetResponse(req, parameters);
        }
        public static string InternalServerError(HttpListenerRequest req, RequestParameters parameters, Exception ex)
        {
            parameters.QueryParameters.Add("&Exception", ex.Message);
            return internalServerError.GetResponse(req, parameters);
        }
    }
}
