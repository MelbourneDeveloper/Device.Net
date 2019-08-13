using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace testusb
{
    class Program
    {
        static void Main(string[] args)
        {

            WindowsUsbDeviceFactory.Register(null, null);
            var x = DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null).Result;
            ;
        }
    }
}
