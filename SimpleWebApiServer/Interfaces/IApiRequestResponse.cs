using System.Net;
using System.Threading.Tasks;

namespace SimpleWebApiServer.Interfaces
{
    public interface IApiRequestResponse
    {
        HttpListenerRequestCache Cache { get; set; }
        string HttpMethod { get; }
        bool IsCacheable { get; }
        string UrlPattern { get; }

        Task<string> GetRequestAsync(HttpListenerRequest request, RequestParameters parameters);
        Task<string> PostRequestAsync(HttpListenerRequest request, RequestParameters parameters, string postData);

        string ToJson(object obj);
        T FromJson<T>(string json);
    }
}