using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleWebApiServer
{
    public class RequestParameters
    {
        public Dictionary<string, string> _urlParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> _queryParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public RequestParameters()
        {

        }
        public string GetUrlParameter(string parameter)
        {
            if (_urlParameters.ContainsKey(parameter))
            {
                return _urlParameters[parameter];
            }
            return string.Empty;
        }

        public string GetQueryParameter(string parameter)
        {
            if (_queryParameters.ContainsKey(parameter))
            {
                return _queryParameters[parameter];
            }
            return string.Empty;
        }

        public RequestParameters(Dictionary<string, string> urlParameters)
        {
            _urlParameters = new Dictionary<string, string>(urlParameters, StringComparer.OrdinalIgnoreCase);
        }
        public RequestParameters(Dictionary<string, string> urlParameters, Dictionary<string, string> queryParameters)
        {
            _urlParameters = new Dictionary<string, string>(urlParameters, StringComparer.OrdinalIgnoreCase);
            _queryParameters = new Dictionary<string, string>(queryParameters, StringComparer.OrdinalIgnoreCase);        }
        public Dictionary<string, string> UrlParameters { get { return _urlParameters; } }
        public Dictionary<string, string> QueryParameters { get { return _queryParameters; } }
    }
}
