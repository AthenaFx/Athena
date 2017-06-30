using System;

namespace Athena.EventStore.Projections
{
    public class ProjectionFailed
    {
        public ProjectionFailed(Type projection, Exception exception)
        {
            Projection = projection;
            Exception = exception;
        }

        public Type Projection { get; }
        public Exception Exception { get; }
    }
}