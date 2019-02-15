using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public abstract class WindowsDeviceFactoryBase
    {
        #region Public Abstract Properties
        public abstract DeviceType DeviceType { get; }
        public abstract Guid ClassGuid { get; set; }
        #endregion

        #region Protected Abstract Methods
        protected abstract ConnectedDeviceDefinition GetDeviceDefinition(string deviceId);
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return await Task.Run<IEnumerable<ConnectedDeviceDefinition>>(() =>
            {
                var deviceDefinitions = new Collection<ConnectedDeviceDefinition>();
                var spDeviceInterfaceData = new SpDeviceInterfaceData();
                var spDeviceInfoData = new SpDeviceInfoData();
                var spDeviceInterfaceDetailData = new SpDeviceInterfaceDetailData();
                spDeviceInterfaceData.CbSize = (uint)Marshal.SizeOf(spDeviceInterfaceData);
                spDeviceInfoData.CbSize = (uint)Marshal.SizeOf(spDeviceInfoData);

                var guidString = ClassGuid.ToString();
                var copyOfClassGuid = new Guid(guidString);

                var devicesHandle = APICalls.SetupDiGetClassDevs(ref copyOfClassGuid, IntPtr.Zero, IntPtr.Zero, APICalls.DigcfDeviceinterface | APICalls.DigcfPresent);

                if (IntPtr.Size == 8)
                {
                    spDeviceInterfaceDetailData.CbSize = 8;
                }
                else
                {
                    spDeviceInterfaceDetailData.CbSize = 4 + Marshal.SystemDefaultCharSize;
                }

                var i = -1;

                var productIdHex = GetHex(deviceDefinition.ProductId);
                var vendorHex = GetHex(deviceDefinition.VendorId);

                while (true)
                {
                    i++;

                    var isSuccess = APICalls.SetupDiEnumDeviceInterfaces(devicesHandle, IntPtr.Zero, ref copyOfClassGuid, (uint)i, ref spDeviceInterfaceData);
                    if (!isSuccess)
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == APICalls.ERROR_NO_MORE_ITEMS)
                        {
                            break;
                        }

                        throw new Exception($"Could not enumerate devices. Error code: {errorCode}");
                    }

                    isSuccess = APICalls.SetupDiGetDeviceInterfaceDetail(devicesHandle, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);
                    WindowsDeviceBase.HandleError(isSuccess, "Could not get device interface detail");

                    //Note this is a bit nasty but we can filter Vid and Pid this way I think...
                    if (deviceDefinition.VendorId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(vendorHex)) continue;
                    if (deviceDefinition.ProductId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(productIdHex)) continue;

                    var connectedDeviceDefinition = GetDeviceDefinition(spDeviceInterfaceDetailData.DevicePath);

                    if (connectedDeviceDefinition == null) continue;

                    if (!DeviceManager.IsDefinitionMatch(deviceDefinition, connectedDeviceDefinition)) continue;

                    deviceDefinitions.Add(connectedDeviceDefinition);
                }

                APICalls.SetupDiDestroyDeviceInfoList(devicesHandle);

                return deviceDefinitions;
            });
        }
        #endregion

        #region Private Static Methods
        private static string GetHex(uint? id)
        {
            return id?.ToString("X").ToLower().PadLeft(4, '0');
        }
        private static uint GetNumberFromDeviceId(string deviceId, string searchString)
        {
            var indexOfSearchString = deviceId.ToLower().IndexOf(searchString);
            string hexString = null;
            if (indexOfSearchString > -1)
            {
                hexString = deviceId.Substring(indexOfSearchString + searchString.Length, 4);
            }
            var numberAsInteger = uint.Parse(hexString, NumberStyles.HexNumber);
            return numberAsInteger;
        }
        #endregion

        #region Public Static Methods
        public static ConnectedDeviceDefinition GetDeviceDefinitionFromWindowsDeviceId(string deviceId, DeviceType deviceType)
        {
            uint? vid = null;
            uint? pid = null;
            try
            {
                vid = GetNumberFromDeviceId(deviceId, "vid_");
                pid = GetNumberFromDeviceId(deviceId, "pid_");
            }
            catch (Exception)
            {
                //TODO: Logging
                //We really need the Vid/Pid here for polling etc. so not sure if swallowing errors it the way to go
            }

            return new ConnectedDeviceDefinition(deviceId) { DeviceType = deviceType, VendorId = vid, ProductId = pid };
        }
        #endregion
    }
}
