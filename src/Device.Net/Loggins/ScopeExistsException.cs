# if !NET45

using System;
using System.Runtime.Serialization;

namespace CallbackLogger
{
    [Serializable]
    public class ScopeExistsException : Exception
    {
        public ScopeExistsException()
        {
        }

        public ScopeExistsException(string message) : base(message)
        {
        }

        public ScopeExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScopeExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

#endif