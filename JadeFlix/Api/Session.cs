using System.Net;
using SimpleWebApiServer;
using System;
using System.Collections.Concurrent;
using JadeFlix.Domain;
using JadeFlix.Domain.ApiParameters;
using System.Threading.Tasks;

namespace JadeFlix.Api
{
    public class Session : ApiRequestResponse<SessionApiParams>
    {
        public static ConcurrentDictionary<string, string> _session = new ConcurrentDictionary<string, string>();
        public Session(HttpListenerRequest cache = null) : base("/api/session") { }
        public override bool IsCacheable => false;

        public override string HttpMethod => "GET|POST";

        protected override async Task<string> ProcessGetRequest(HttpListenerRequest request, SessionApiParams apiParams)
        {
            var sessionKey = GetSessionKey(apiParams.SessionId, request, apiParams.Key);
            var sessionResponse = new SessionResponse();

            sessionResponse = GetSessionValue(apiParams, sessionKey);

            await Task.Delay(10);
            if (sessionResponse.Result == null)
            {
                return string.Empty;
            }
            return ToJson(sessionResponse);
        }
        protected override async Task<string> ProcessPostRequest(HttpListenerRequest request, SessionApiParams apiParams, string postData)
        {
            var sessionKey = GetSessionKey(apiParams.SessionId, request, apiParams.Key);
            var sessionResponse = new SessionResponse();
            apiParams.Value = postData;

            sessionResponse = SetSessionValue(apiParams, sessionKey);

            await Task.Delay(10);
            return ToJson(sessionResponse);
        }
        private static SessionResponse SetSessionValue(SessionApiParams apiParams, string sessionKey)
        {
            var sessionResponse = new SessionResponse();
            Console.WriteLine("Creating new session key: " + sessionKey);
            _session.AddOrUpdate(sessionKey, apiParams.Value, (oldItem, newItem) => { return apiParams.Value; });
            sessionResponse.Result = "200";

            return sessionResponse;
        }

        private static SessionResponse GetSessionValue(SessionApiParams apiParams, string sessionKey)
        {
            var sessionResponse = new SessionResponse();
            if (_session.ContainsKey(sessionKey))
            {
                Console.WriteLine("Getting session value for: " + apiParams.Key);
                sessionResponse.Result = _session[sessionKey];
            }
            else
            {
                Console.WriteLine("Session value: " + apiParams.Key + " not found");
            }
            return sessionResponse;
        }

        private string GetSessionKey(string sessionId, HttpListenerRequest request, string key)
        {
            return sessionId + "/" + request.RemoteEndPoint.Address.ToString() + "/" + key;
        }

        public override SessionApiParams ParseParameters(RequestParameters parameters)
        {
            return new SessionApiParams
            {
                SessionId = parameters.GetQueryParameter("sessionId"),
                Key = parameters.GetQueryParameter("key"),
                Value = parameters.GetQueryParameter("value")
            };
        }

        
    }

}
