using Device.Net;
using Device.Net.UWP;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Usb.Net.UWP
{
    public class UWPUsbDeviceFactory : UWPDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Usb;
        protected override string VendorFilterName => "System.DeviceInterface.WinUsb.UsbVendorId";
        protected override string ProductFilterName => "System.DeviceInterface.WinUsb.UsbProductId";
        #endregion

        #region Public Properties
        public ushort? ReadBufferSize { get; set; }
        public ushort? WriteBufferSize { get; set; }
        #endregion

        #region Protected Override Methods
        protected override string GetAqsFilter(uint? vendorId, uint? productId)
        {
            //TODO: This is hard coded for WinUSB devices. Can we use other types of devices? GPS devices for example?
            var interfaceClassGuid = "System.Devices.InterfaceClassGuid:=\"{" + WindowsDeviceConstants.WinUSBGuid + "}\"";
            return $"{interfaceClassGuid} {InterfaceEnabledPart} {GetVendorPart(vendorId)} {GetProductPart(productId)}";
        }
        #endregion

        #region Constructor
        public UWPUsbDeviceFactory(ILoggerFactory loggerFactory) : base(loggerFactory, loggerFactory.CreateLogger<UWPUsbDeviceFactory>())
        {
        }
        #endregion

        #region Public Methods
        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return deviceDefinition == null
                ? throw new ArgumentNullException(nameof(deviceDefinition))
                : deviceDefinition.DeviceType == DeviceType.Hid ? null :
                new UsbDevice(deviceDefinition.DeviceId,
                new UWPUsbInterfaceManager(
                    deviceDefinition,
                    LoggerFactory,
                    ReadBufferSize,
                    WriteBufferSize), Logger);
        }
        #endregion

        #region Public Overrides
        public override Task<ConnectionInfo> TestConnection(string deviceId) => Task.FromResult(new ConnectionInfo { CanConnect = true });
        #endregion
    }
}

