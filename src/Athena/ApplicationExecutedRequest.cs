using System;

namespace Athena
{
    public class ApplicationExecutedRequest
    {
        public ApplicationExecutedRequest(string application, string requestId, TimeSpan duration, DateTime at)
        {
            Application = application;
            RequestId = requestId;
            Duration = duration;
            At = at;
        }

        public string Application { get; }
        public string RequestId { get; }
        public TimeSpan Duration { get; }
        public DateTime At { get; }
    }
}