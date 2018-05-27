using SimpleWebApiServer.ServerResponses;
using System;
using System.Net;

namespace SimpleWebApiServer
{
    internal static class Responses
    {
        private static readonly NotFoundResponse NotFoundresponse = new NotFoundResponse();
        private static readonly InternalServerError InternalServerErrorResponse = new InternalServerError();
        public static string NotFound(HttpListenerRequest req, RequestParameters parameters)
        {
            return NotFoundresponse.GetResponse(req, parameters);
        }
        public static string InternalServerError(HttpListenerRequest req, RequestParameters parameters, Exception ex)
        {
            parameters.QueryParameters.Add("&Exception", ex.Message);
            return InternalServerErrorResponse.GetResponse(req, parameters);
        }
    }
}
