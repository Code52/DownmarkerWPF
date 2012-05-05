using System;

namespace MarkPad.Services.Metaweblog.Rsd
{
    public class DiscoveryResult
    {
        public DiscoveryResult(string webApiLink)
        {
            Success = true;
            MetaWebLogApiLink = webApiLink;
        }

        public DiscoveryResult(Exception ex)
        {
            Success = false;
            if (ex is AggregateException)
            {
                ex = ((AggregateException) ex).Flatten().InnerException;
            }
            Exception = ex;
            FailMessage = ex.Message;
        }

        public bool Success { get; private set; }
        public Exception Exception { get; private set; }
        public string MetaWebLogApiLink { get; private set; }
        public string FailMessage { get; private set; }

        public static DiscoveryResult Failed(string failedMessage)
        {
            return new DiscoveryResult((Exception)null)
                       {
                           FailMessage = failedMessage
                       };
        }
    }
}