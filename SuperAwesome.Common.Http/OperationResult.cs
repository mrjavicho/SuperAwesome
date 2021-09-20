namespace SuperAwesome.Common.Http
{
    public class OperationResult<T>
    {
        public RequestInfo RequestInfo { get; set; }
        public T Response { get; set; }
        public bool IsSuccess { get; set; }
    }
}