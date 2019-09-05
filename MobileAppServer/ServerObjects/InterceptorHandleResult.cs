namespace MobileAppServer.ServerObjects
{
    public class InterceptorHandleResult
    {
        internal bool CancelActionInvoke { get; set; }
        public bool ResponseSuccess { get; }
        internal string Message { get; set; }

        internal object Data { get; set; }

        public InterceptorHandleResult(bool cancelActionInvoke,
            bool responseSuccess,
            string message, object data)
        {
            CancelActionInvoke = cancelActionInvoke;
            ResponseSuccess = responseSuccess;
            Message = message;
            Data = data;
        }
    }
}
