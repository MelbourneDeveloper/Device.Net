using Flutnet.ServiceModel;
using System;

namespace FlutnetThermometer.ServiceLibrary
{
    [PlatformData]
    public class TemperatureChangedArgs : EventArgs
    {
        public double Celsius { get; set; }

        public double Fahrenheit { get; set; }
    }
}


