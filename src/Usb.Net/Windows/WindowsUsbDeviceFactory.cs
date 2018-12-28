using Device.Net;
using Device.Net.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceFactory : IDeviceFactory
    {
        #region Public Properties
        public DeviceType DeviceType => DeviceType.Usb;
        public Guid ClassGuid { get; set; } = WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE;
        #endregion

        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsUsbDeviceFactory());
        }

        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceType != DeviceType.Usb) return null;
            return new WindowsUsbDevice(deviceDefinition.DeviceId);
        }

        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            return await Task.Run<IEnumerable<DeviceDefinition>>(() =>
            {
                var DeviceDefinitions = new Collection<DeviceDefinition>();
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

                    var setupDiEnumDeviceInterfacesResult = APICalls.SetupDiEnumDeviceInterfaces(i, IntPtr.Zero, ref copyOfClassGuid, (uint)x, ref spDeviceInterfaceData);
                    var errorNumber = Marshal.GetLastWin32Error();

                    //TODO: deal with error numbers. Give a meaningful error message

                    if (setupDiEnumDeviceInterfacesResult == false)
                    {
                        break;
                    }

                    APICalls.SetupDiGetDeviceInterfaceDetail(i, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);

                    //TODO: Super duplication here. Merge!!!
                    var vendorHex = vendorId?.ToString("X").ToLower().PadLeft(4, '0');
                    var productIdHex = productId?.ToString("X").ToLower().PadLeft(4, '0');
                    if (vendorId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(vendorHex)) continue;
                    if (productId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(productIdHex)) continue;

                    var DeviceDefinition = new DeviceDefinition { DeviceId = spDeviceInterfaceDetailData.DevicePath, DeviceType = DeviceType.Usb };

                    DeviceDefinitions.Add(DeviceDefinition);
                }

                APICalls.SetupDiDestroyDeviceInfoList(i);

                return DeviceDefinitions;
            });
        }
    }
}
