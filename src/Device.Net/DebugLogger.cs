using System;
using System.Diagnostics;

namespace Device.Net
{
    public class DebugLogger : ILogger
    {
        public bool LogToConsole { get; set; }

        public void Log(string message, string region, Exception ex, LogLevel logLevel)
        {
            var formattedMessage = $"Message: {message}\r\nTime: {DateTime.Now}\r\nSection: {region}\r\nError: {ex}";
            formattedMessage = $"--------------------------------------\r\n{formattedMessage}\r\n--------------------------------------";
            if (LogToConsole)
            {
                Console.WriteLine(formattedMessage);
            }
            else
            {
                Debug.WriteLine(formattedMessage);
            }
        }
    }
}