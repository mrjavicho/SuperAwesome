using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperAwesome.Common.Http
{
    public class HttpRequestService : IHttpRequestService
    {
        public HttpClient Client { get; private set; }

        public HttpRequestHeaders HttpRequestHeaders => Client?.DefaultRequestHeaders;


        public Task<OperationResult<TResult>> Get<TResult>(string uri, CancellationToken token)
        {
            return ExecuteWebRequest<TResult>(RequestType.Get, uri, null, token);
        }

        public Task<OperationResult<TResult>> Post<TResult>(string uri, object payload, CancellationToken token)
        {
            return ExecuteWebRequest<TResult>(RequestType.Post, uri, payload, token);
        }

        public Task<OperationResult<TResult>> Put<TResult>(string uri, object payload, CancellationToken token)
        {
            return ExecuteWebRequest<TResult>(RequestType.Put, uri, payload, token);
        }

        public Task<OperationResult<TResult>> Delete<TResult>(string uri, CancellationToken token)
        {
            return ExecuteWebRequest<TResult>(RequestType.Delete, uri, null, token);
        }
        
         private async Task<OperationResult<TResult>> ExecuteWebRequest<TResult>(RequestType requestType, string endpoint, object payload, CancellationToken token)
         {
             var requestUrl = Client.BaseAddress + endpoint;
             StringContent content = null;
             if (payload is string textPayload && !string.IsNullOrEmpty(textPayload))
             {
                 content = new StringContent(textPayload, Encoding.UTF8, "application/json");
             }
             else if (payload != null)
             {
                 var serializedPayload = JsonConvert.SerializeObject(payload);
                 content = new StringContent(serializedPayload, Encoding.UTF8, "application/json");    
             }
             else
             {
                 content = new StringContent("");
             }
             
             
             try
             { 
                 HttpResponseMessage responseMessage;
                switch (requestType)
                {
                    case RequestType.Get:
                        responseMessage = await Client.GetAsync(requestUrl, token);
                        break;
                    case RequestType.Put:
                        responseMessage = await Client.PutAsync(requestUrl, content, token);
                        break;
                    case RequestType.Post:
                        responseMessage = await Client.PostAsync(requestUrl, content, token);
                        break;
                    case RequestType.Delete:
                        responseMessage = await Client.DeleteAsync(requestUrl, token);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null);
                }

                var responseString = await responseMessage.Content.ReadAsStringAsync();

                var opResult = new OperationResult<TResult>()
                {
                    IsSuccess = responseMessage.IsSuccessStatusCode,
                    RequestInfo = new CompletedRequestInfo()
                    {
                        Url = requestUrl,
                        Response = responseString,
                        ResponseCode = responseMessage.StatusCode,
                        RequestType = requestType
                    }
                };

                if (responseMessage.IsSuccessStatusCode)
                {
                    opResult.Response = JsonConvert.DeserializeObject<TResult>(responseString);
                }
                
                LogRequest(requestType, requestUrl, responseMessage.StatusCode, responseString);

                return opResult;
            }
            catch (Exception e)
            {
                LogError(requestType, requestUrl, e);
                return new OperationResult<TResult>
                {
                    IsSuccess = false,
                    RequestInfo = new FaultedRequestInfo()
                    {
                        Url = requestUrl,
                        RequestType = requestType,
                        Exception = e
                    }
                };
            }
        }

        public void Initialize(string baseAddress, HttpClientHandler handler = null)
        {
            Client = handler != null ? new HttpClient(handler) : new HttpClient();
            Client.BaseAddress = new Uri(baseAddress);
        }

        private void LogRequest(RequestType type, string requestUri, HttpStatusCode statusCode, string responseMessage)
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine($"[REQUEST ({type.ToString()})] {requestUri}");
            logBuilder.AppendLine($"[HEADERS] {Client.DefaultRequestHeaders.Authorization}");
            logBuilder.AppendLine($"[RESPONSE {statusCode}] {responseMessage}");
            System.Diagnostics.Debug.WriteLine(logBuilder.ToString());
        }
        
        private void LogError(RequestType type, string requestUri, Exception exception)
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine($"[REQUEST ({type.ToString()})] {requestUri}");
            logBuilder.AppendLine($"[FAILED: {exception.Source}]");
            logBuilder.AppendLine(exception.StackTrace);
            System.Diagnostics.Debug.WriteLine(logBuilder.ToString());
        }
    }
}