using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    public abstract class WindowsDeviceFactoryBase
    {
        #region Public Abstract Properties
        public abstract DeviceType DeviceType { get; }
        public abstract Guid ClassGuid { get; set; }
        #endregion

        #region Protected Abstract Methods
        protected abstract DeviceDefinition GetDeviceDefinition(string deviceId);
        #endregion

        #region Public Methods
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

                var productIdHex = GetHex(productId);
                var vendorHex = GetHex(vendorId);

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

                        throw new Exception($"Could not enumerate devices. Error code: {errorCode}");
                    }

                    isSuccess = APICalls.SetupDiGetDeviceInterfaceDetail(i, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);
                    WindowsDeviceBase.HandleError(isSuccess, "Could not get device interface detail");

                    //Note this is a bit nasty but we can filter Vid and Pid this way I think...
                    if (vendorId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(vendorHex)) continue;
                    if (productId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(productIdHex)) continue;

                    deviceDefinitions.Add(GetDeviceDefinition(spDeviceInterfaceDetailData.DevicePath));
                }

                APICalls.SetupDiDestroyDeviceInfoList(i);

                return deviceDefinitions;
            });
        }
        #endregion

        #region Private Static Methods
        private static string GetHex(uint? id)
        {
            return id?.ToString("X").ToLower().PadLeft(4, '0');
        }
        #endregion
    }
}
