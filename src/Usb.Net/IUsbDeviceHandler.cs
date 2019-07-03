using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Usb.Net
{
    public interface IUsbDeviceHandler : IDisposable
    {
        IUsbInterface ReadUsbInterface { get; set; }
        IList<IUsbInterface> UsbInterfaces { get; }
        IUsbInterface WriteUsbInterface { get; set; }
        Task InitializeAsync();
    }
}