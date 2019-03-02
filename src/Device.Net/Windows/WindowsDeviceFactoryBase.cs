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
        #region Public Properties
        public ILogger Logger { get; set; }
        #endregion

        #region Public Abstract Properties
        public abstract DeviceType DeviceType { get; }
        #endregion

        #region Protected Abstract Methods
        protected abstract ConnectedDeviceDefinition GetDeviceDefinition(string deviceId);
        protected abstract Guid GetClassGuid();
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition filterDeviceDefinition)
        {
            return await Task.Run<IEnumerable<ConnectedDeviceDefinition>>(() =>
            {
                var deviceDefinitions = new Collection<ConnectedDeviceDefinition>();
                var spDeviceInterfaceData = new SpDeviceInterfaceData();
                var spDeviceInfoData = new SpDeviceInfoData();
                var spDeviceInterfaceDetailData = new SpDeviceInterfaceDetailData();
                spDeviceInterfaceData.CbSize = (uint)Marshal.SizeOf(spDeviceInterfaceData);
                spDeviceInfoData.CbSize = (uint)Marshal.SizeOf(spDeviceInfoData);

                var guidString = GetClassGuid().ToString();
                var copyOfClassGuid = new Guid(guidString);

                var devicesHandle = APICalls.SetupDiGetClassDevs(ref copyOfClassGuid, IntPtr.Zero, IntPtr.Zero, APICalls.DigcfDeviceinterface | APICalls.DigcfPresent);

                spDeviceInterfaceDetailData.CbSize = IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize;

                var i = -1;

                var productIdHex = Helpers.GetHex(filterDeviceDefinition.ProductId);
                var vendorHex = Helpers.GetHex(filterDeviceDefinition.VendorId);

                while (true)
                {
                    try
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

                            if (errorCode > 0) continue;
                        }

                        isSuccess = APICalls.SetupDiGetDeviceInterfaceDetail(devicesHandle, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);
                        if (!isSuccess)
                        {
                            var errorCode = Marshal.GetLastWin32Error();

                            if (errorCode == APICalls.ERROR_NO_MORE_ITEMS)
                            {
                                break;
                            }

                            if (errorCode > 0) continue;
                        }

                        //Note this is a bit nasty but we can filter Vid and Pid this way I think...
                        if (filterDeviceDefinition.VendorId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ContainsIgnoreCase(vendorHex)) continue;
                        if (filterDeviceDefinition.ProductId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ContainsIgnoreCase(productIdHex)) continue;

                        var connectedDeviceDefinition = GetDeviceDefinition(spDeviceInterfaceDetailData.DevicePath);

                        if (connectedDeviceDefinition == null) continue;

                        if (!DeviceManager.IsDefinitionMatch(filterDeviceDefinition, connectedDeviceDefinition)) continue;

                        deviceDefinitions.Add(connectedDeviceDefinition);
                    }
                    catch (Exception ex)
                    {
                        Logger?.Log("Error", nameof(WindowsDeviceFactoryBase), ex, LogLevel.Error);
                    }
                }

                APICalls.SetupDiDestroyDeviceInfoList(devicesHandle);

                return deviceDefinitions;
            });
        }
        #endregion

        #region Private Static Methods
        private static uint GetNumberFromDeviceId(string deviceId, string searchString)
        {
            var indexOfSearchString = deviceId.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);
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
