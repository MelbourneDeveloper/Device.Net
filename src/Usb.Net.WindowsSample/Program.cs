using System.Threading;
using Usb.Net.WindowsSample.Temperature;


namespace Usb.Net.WindowsSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var temperatureMonitor = new TemperatureMonitor();
            var temperaturReporter = new TemperatureReporter();
            temperaturReporter.Subscribe(temperatureMonitor);

            while (true)
            {
                Thread.Sleep(1500);
                temperatureMonitor.GetTemperature();
            }

        }
    }
}