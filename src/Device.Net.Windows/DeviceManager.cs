using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net
{
    public class DeviceManager
    {
        #region Public Static Properties
        public static DeviceManager Current { get; } = new DeviceManager();
        #endregion

        #region Public Methods
        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId, DeviceType deviceType)
        {
            return await Task.Run<IEnumerable<DeviceDefinition>>(() =>
            {
                var deviceInformations = new Collection<DeviceDefinition>();
                var spDeviceInterfaceData = new SpDeviceInterfaceData();
                var spDeviceInfoData = new SpDeviceInfoData();
                var spDeviceInterfaceDetailData = new SpDeviceInterfaceDetailData();
                spDeviceInterfaceData.CbSize = (uint)Marshal.SizeOf(spDeviceInterfaceData);
                spDeviceInfoData.CbSize = (uint)Marshal.SizeOf(spDeviceInfoData);

                var classGuid = deviceType == DeviceType.Hid ? WindowsDeviceConstants.GUID_DEVINTERFACE_HID : WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE;

                var i = APICalls.SetupDiGetClassDevs(ref classGuid, IntPtr.Zero, IntPtr.Zero, APICalls.DigcfDeviceinterface | APICalls.DigcfPresent);

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

                    var setupDiEnumDeviceInterfacesResult = APICalls.SetupDiEnumDeviceInterfaces(i, IntPtr.Zero, ref classGuid, (uint)x, ref spDeviceInterfaceData);
                    var errorNumber = Marshal.GetLastWin32Error();

                    //TODO: deal with error numbers. Give a meaningful error message

                    if (setupDiEnumDeviceInterfacesResult == false)
                    {
                        break;
                    }

                    APICalls.SetupDiGetDeviceInterfaceDetail(i, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);

                    var deviceInformation = new DeviceDefinition { DeviceId = spDeviceInterfaceDetailData.DevicePath };

                    deviceInformations.Add(deviceInformation);
                }

                APICalls.SetupDiDestroyDeviceInfoList(i);

                return deviceInformations;
            });
        }

        #endregion
    }
}
