using System;
using System.Runtime.CompilerServices;

namespace Device.Net

{
    public class Logger : ILogger
    {
        public void Log(string message, Exception ex, string region, [CallerMemberName] string callerMemberName = null)
        {
            Log(message, $"{region} - Calling Member: {callerMemberName}", ex, ex != null ? LogLevel.Error : LogLevel.Information);
        }

        public void Log(string message, string region, Exception ex, LogLevel logLevel)
        {
            var formattedText = $"Message: {message}\r\nTime: {DateTime.Now}\r\nSection: {region}\r\nError: {ex}";
            System.Diagnostics.Debug.WriteLine($"--------------------------------------\r\n{formattedText}\r\n--------------------------------------");
        }
    }
}
