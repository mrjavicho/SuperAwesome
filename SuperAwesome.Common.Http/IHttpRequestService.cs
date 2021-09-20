using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SuperAwesome.Common.Http
{
    public interface IHttpRequestService
    {
        HttpClient Client { get; }
        void Initialize(string baseAddress, HttpClientHandler handler = null);
        Task<OperationResult<TResult>> Get<TResult>(string uri, CancellationToken token);
        Task<OperationResult<TResult>> Post<TResult>(string uri, object payload, CancellationToken token);
        Task<OperationResult<TResult>> Put<TResult>(string uri, object payload, CancellationToken token);
        Task<OperationResult<TResult>> Delete<TResult>(string uri, CancellationToken token);
    }
}