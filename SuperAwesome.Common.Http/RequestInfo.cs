using System;
using System.Net;

namespace SuperAwesome.Common.Http
{
    public abstract class RequestInfo{
        public RequestType RequestType { get; set; }
        public string  Url { get; set; }
    }

    public class CompletedRequestInfo : RequestInfo
    {
        public string Payload { get; set; }
        public string Response { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
    }

    public class FaultedRequestInfo : RequestInfo
    {
        public Exception Exception { get; set; }          
    }
}