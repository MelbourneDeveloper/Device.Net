using Device.Net;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        public ushort? ReadBufferSize { get; set; }
        public ushort? WriteBufferSize { get; set; }

        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Usb;
        #endregion

        #region Protected Override Methods

        /// <summary>
        /// The parent Guid to enumerate through devices at. This probably shouldn't be changed. It defaults to the WinUSB Guid. 
        /// </summary>
        protected override Guid GetClassGuid() => WindowsDeviceConstants.WinUSBGuid;
        #endregion

        #region Constructor
        public WindowsUsbDeviceFactory(ILoggerFactory loggerFactory) : base(loggerFactory, loggerFactory.CreateLogger<WindowsUsbDeviceFactory>())
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return deviceDefinition == null
                ? throw new ArgumentNullException(nameof(deviceDefinition))
                : deviceDefinition.DeviceType != DeviceType ? null :
                new UsbDevice(deviceDefinition.DeviceId,
                    new WindowsUsbInterfaceManager(
                    deviceDefinition.DeviceId,
                        LoggerFactory,
                        ReadBufferSize,
                        WriteBufferSize)
                , LoggerFactory.CreateLogger<UsbDevice>());
        }
        #endregion

        #region Private Static Methods
        protected override ConnectedDeviceDefinition GetDeviceDefinition(string deviceId) => DeviceBase.GetDeviceDefinitionFromWindowsDeviceId(deviceId, DeviceType.Usb, Logger);
        #endregion

        #region Public Static Methods
        public static ConnectedDeviceDefinition GetDeviceDefinition(SafeFileHandle defaultInterfaceHandle, string deviceId)
        {
            var deviceDefinition = new ConnectedDeviceDefinition(deviceId) { DeviceType = DeviceType.Usb };

            var bufferLength = (uint)Marshal.SizeOf(typeof(USB_DEVICE_DESCRIPTOR));
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var isSuccess2 = WinUsbApiCalls.WinUsb_GetDescriptor(defaultInterfaceHandle, WinUsbApiCalls.DEFAULT_DESCRIPTOR_TYPE, 0, WinUsbApiCalls.EnglishLanguageID, out var _UsbDeviceDescriptor, bufferLength, out var lengthTransferred);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            WindowsDeviceBase.HandleError(isSuccess2, "Couldn't get device descriptor");

            if (_UsbDeviceDescriptor.iProduct > 0)
            {
                deviceDefinition.ProductName = WinUsbApiCalls.GetDescriptor(defaultInterfaceHandle, _UsbDeviceDescriptor.iProduct, "Couldn't get product name");
            }

            if (_UsbDeviceDescriptor.iSerialNumber > 0)
            {
                deviceDefinition.SerialNumber = WinUsbApiCalls.GetDescriptor(defaultInterfaceHandle, _UsbDeviceDescriptor.iSerialNumber, "Couldn't get serial number");
            }

            if (_UsbDeviceDescriptor.iManufacturer > 0)
            {
                deviceDefinition.Manufacturer = WinUsbApiCalls.GetDescriptor(defaultInterfaceHandle, _UsbDeviceDescriptor.iManufacturer, "Couldn't get manufacturer");
            }

            deviceDefinition.VendorId = _UsbDeviceDescriptor.idVendor;
            deviceDefinition.ProductId = _UsbDeviceDescriptor.idProduct;
            deviceDefinition.WriteBufferSize = _UsbDeviceDescriptor.bMaxPacketSize0;
            deviceDefinition.ReadBufferSize = _UsbDeviceDescriptor.bMaxPacketSize0;

            return deviceDefinition;
        }
        #endregion
    }
}
