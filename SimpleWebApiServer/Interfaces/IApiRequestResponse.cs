using System.Net;

namespace SimpleWebApiServer.Interfaces
{
    public interface IApiRequestResponse
    {
        HttpListenerRequestCache Cache { get; set; }
        string HttpMethod { get; }
        bool IsCacheable { get; }
        string UrlPattern { get; }

        string GetRequest(HttpListenerRequest request, RequestParameters parameters);
        string PostRequest(HttpListenerRequest request, RequestParameters parameters, string postData);

        string ToJson(object obj);
        T FromJson<T>(string json);
    }
}