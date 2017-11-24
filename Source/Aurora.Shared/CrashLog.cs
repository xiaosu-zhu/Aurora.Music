namespace Com.Aurora.AuWeather.Shared
{
    public class CrashLog
    {
        public string Exception;
        public int HResult;
        public string StackTrace;
        public string Source;
        public string Message;

        public CrashLog(string exception, int hResult, string stackTrace, string source, string message)
        {
            Exception = exception;
            HResult = hResult;
            StackTrace = stackTrace;
            Source = source;
            Message = message;
        }
    }
}
