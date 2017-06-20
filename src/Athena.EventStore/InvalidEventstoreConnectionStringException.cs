using System;

namespace Athena.EventStore
{
    public class InvalidEventstoreConnectionStringException : Exception
    {
        public InvalidEventstoreConnectionStringException(string message)
            : base(message)
        {

        }
    }
}