using System;

namespace Device.Net.Exceptions
{
    public class AsyncException : Exception
    {
        public AsyncException(string message) : base(message)
        {
        }

        public AsyncException()
        {
        }

        public AsyncException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}