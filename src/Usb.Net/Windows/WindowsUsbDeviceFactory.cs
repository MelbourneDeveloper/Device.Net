﻿using Device.Net;
using Device.Net.Windows;
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
        public WindowsUsbDeviceFactory(ILogger logger, ITracer tracer) : base(logger, tracer)
        {
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));

            return deviceDefinition.DeviceType != DeviceType ? null : new UsbDevice(new WindowsUsbInterfaceManager(deviceDefinition.DeviceId, Logger, Tracer, ReadBufferSize, WriteBufferSize), Logger, Tracer);
        }
        #endregion

        #region Private Static Methods
        protected override ConnectedDeviceDefinition GetDeviceDefinition(string deviceId)
        {
            return GetDeviceDefinitionFromWindowsDeviceId(deviceId, DeviceType.Usb, Logger);
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Register the factory for enumerating USB devices in Windows.
        /// </summary>
        public static void Register(ILogger logger, ITracer tracer)
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsUsbDeviceFactory(logger, tracer));
        }

        public static ConnectedDeviceDefinition GetDeviceDefinition(SafeFileHandle defaultInterfaceHandle, string deviceId)
        {
            var deviceDefinition = new ConnectedDeviceDefinition(deviceId) { DeviceType = DeviceType.Usb };

            var bufferLength = (uint)Marshal.SizeOf(typeof(USB_DEVICE_DESCRIPTOR));
            var isSuccess2 = WinUsbApiCalls.WinUsb_GetDescriptor(defaultInterfaceHandle, WinUsbApiCalls.DEFAULT_DESCRIPTOR_TYPE, 0, WinUsbApiCalls.EnglishLanguageID, out var _UsbDeviceDescriptor, bufferLength, out var lengthTransferred);
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
            deviceDefinition.DeviceClass = _UsbDeviceDescriptor.bDeviceClass;
            deviceDefinition.DeviceSubClass = _UsbDeviceDescriptor.bDeviceSubClass;
            deviceDefinition.DeviceProtocol = _UsbDeviceDescriptor.bDeviceProtocol;

            return deviceDefinition;
        }
        #endregion
    }
}
