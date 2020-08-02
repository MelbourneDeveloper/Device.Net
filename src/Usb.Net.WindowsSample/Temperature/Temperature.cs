using System;

namespace Usb.Net.WindowsSample.Temperature
{
    public class Temperature
    {
        public Temperature(decimal temperature, DateTime dateAndTime)
        {
            Degrees = temperature;
            Date = dateAndTime;
        }

        public decimal Degrees { get; }

        public DateTime Date { get; }
    }
}
