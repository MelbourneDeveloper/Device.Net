using Flutnet.ServiceModel;
using System;
using System.Threading.Tasks;

namespace FlutnetThermometer.ServiceLibrary
{
    [PlatformService]
    public abstract class ThermometerService
    {

        InfinityTask _monitorTask;

        protected ThermometerService()
        {
            _monitorTask = new InfinityTask("themperature_monitor_task", async () =>
            {
                // Get the temperature
                double celsius = await GetTemperatureAsync();

                // Obtain fahrenheit from celsius
                double fahrenheit = TempConverter.ConvertCelsiusToFahrenheit(celsius);

                // Update the temp value
                OnTemperatureChanged(celsius, fahrenheit);

            }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Temperature changed
        /// </summary>
        [PlatformEvent]
        public event EventHandler<TemperatureChangedArgs> TemperatureChanged;

        /// <summary>
        /// Implement the read code in Android
        /// </summary>
        /// <returns></returns>
        [PlatformOperation]
        public abstract Task<double> GetTemperatureAsync();

        /// <summary>
        /// Update the temperature on Flutter using the event.
        /// </summary>
        /// <param name="celsius"></param>
        public void OnTemperatureChanged(double celsius, double fahrenheit)
        {
            // Send the event to flutter
            TemperatureChanged?.Invoke(this, new TemperatureChangedArgs { Celsius = celsius, Fahrenheit = fahrenheit });
        }

        [PlatformOperation]
        public void StartMonitoring()
        {
            _monitorTask.Start();
        }

        [PlatformOperation]
        public void StopMonitoring()
        {
            _monitorTask.Cancel();
        }

    }

    static class TempConverter
    {
        public static double ConvertCelsiusToFahrenheit(double celsius)
        {
            return ((9.0 / 5.0) * celsius) + 32;
        }

        public static double ConvertFahrenheitToCelsius(double fahrenheit)
        {
            return (5.0 / 9.0) * (fahrenheit - 32);
        }
    }

}