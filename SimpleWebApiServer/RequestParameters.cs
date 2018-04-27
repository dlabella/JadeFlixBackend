using System;
using System.Collections.Generic;

namespace SimpleWebApiServer
{
    public class RequestParameters
    {
        public RequestParameters()
        {

        }
        public string GetUrlParameter(string parameter)
        {
            return UrlParameters.ContainsKey(parameter) ? UrlParameters[parameter] : string.Empty;
        }

        public string GetQueryParameter(string parameter)
        {
            return QueryParameters.ContainsKey(parameter) ? QueryParameters[parameter] : string.Empty;
        }

        public RequestParameters(IDictionary<string, string> urlParameters)
        {
            UrlParameters = new Dictionary<string, string>(urlParameters, StringComparer.OrdinalIgnoreCase);
        }
        public RequestParameters(IDictionary<string, string> urlParameters, IDictionary<string, string> queryParameters)
        {
            UrlParameters = new Dictionary<string, string>(urlParameters, StringComparer.OrdinalIgnoreCase);
            QueryParameters = new Dictionary<string, string>(queryParameters, StringComparer.OrdinalIgnoreCase);        }

        private IDictionary<string, string> UrlParameters { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public IDictionary<string, string> QueryParameters { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
