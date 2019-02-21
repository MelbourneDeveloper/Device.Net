using System;

namespace Hid.Net.Windows
{
    public class WindowsHidException : Exception
    {
        public WindowsHidException(string message) : base(message)
        {

        }

        public WindowsHidException()
        {
        }

        public WindowsHidException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
