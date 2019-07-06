using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Device.Net;

namespace Usb.Net
{
    public interface IUsbDeviceHandler : IDisposable
    {
        IUsbInterface ReadUsbInterface { get; set; }
        IList<IUsbInterface> UsbInterfaces { get; }
        IUsbInterface WriteUsbInterface { get; set; }
        ushort WriteBufferSize { get; }
        ushort ReadBufferSize { get; }
        bool IsInitialized { get; }
        Task InitializeAsync();
        void Close();
        Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync();
    }
}