using Device.Net;
using Device.Net.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Usb;
        //TODO: This is not right
        public override Guid ClassGuid { get; set; } = WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE;
        #endregion

        #region Public Methods
        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsUsbDevice(deviceDefinition.DeviceId);
        }

        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            return await Task.Run<IEnumerable<DeviceDefinition>>(() =>
            {
                var deviceDefinitions = new Collection<DeviceDefinition>();
                var spDeviceInterfaceData = new SpDeviceInterfaceData();
                var spDeviceInfoData = new SpDeviceInfoData();
                var spDeviceInterfaceDetailData = new SpDeviceInterfaceDetailData();
                spDeviceInterfaceData.CbSize = (uint)Marshal.SizeOf(spDeviceInterfaceData);
                spDeviceInfoData.CbSize = (uint)Marshal.SizeOf(spDeviceInfoData);

                var guidString = ClassGuid.ToString();
                var copyOfClassGuid = new Guid(guidString);

                var i = APICalls.SetupDiGetClassDevs(ref copyOfClassGuid, IntPtr.Zero, IntPtr.Zero, APICalls.DigcfDeviceinterface | APICalls.DigcfPresent);

                if (IntPtr.Size == 8)
                {
                    spDeviceInterfaceDetailData.CbSize = 8;
                }
                else
                {
                    spDeviceInterfaceDetailData.CbSize = 4 + Marshal.SystemDefaultCharSize;
                }

                var x = -1;

                while (true)
                {
                    x++;

                    var isSuccess = APICalls.SetupDiEnumDeviceInterfaces(i, IntPtr.Zero, ref copyOfClassGuid, (uint)x, ref spDeviceInterfaceData);
                    if (!isSuccess)
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == APICalls.ERROR_NO_MORE_ITEMS)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception($"Could not enumerate devices. Error code: {errorCode}");
                        }
                    }

                    APICalls.SetupDiGetDeviceInterfaceDetail(i, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);

                    //Note this is a bit nasty but we can filter Vid and Pid this way I think...
                    var vendorHex = vendorId?.ToString("X").ToLower().PadLeft(4, '0');
                    var productIdHex = productId?.ToString("X").ToLower().PadLeft(4, '0');
                    if (vendorId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(vendorHex)) continue;
                    if (productId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(productIdHex)) continue;

                    var DeviceDefinition = new DeviceDefinition { DeviceId = spDeviceInterfaceDetailData.DevicePath, DeviceType = DeviceType.Usb };

                    deviceDefinitions.Add(DeviceDefinition);
                }

                APICalls.SetupDiDestroyDeviceInfoList(i);

                return deviceDefinitions;
            });
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsUsbDeviceFactory());
        }
        #endregion
    }
}
