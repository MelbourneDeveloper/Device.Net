using System;

namespace Device.Net.Windows
{
    public class WindowsException : Exception
    {
        public WindowsException(string message) : base(message)
        {

        }

        public WindowsException()
        {
        }

        public WindowsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
