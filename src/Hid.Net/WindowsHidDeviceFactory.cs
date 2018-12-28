using Device.Net;
using Device.Net.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public class WindowsHidDeviceFactory : WindowsDeviceFactoryBase, IDeviceFactory
    {
        #region Public Override Properties
        public override DeviceType DeviceType => DeviceType.Hid;
        public override Guid ClassGuid { get; set; } = WindowsDeviceConstants.GUID_DEVINTERFACE_HID;
        #endregion

        #region Public Methods
        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            return deviceDefinition.DeviceType != DeviceType ? null : new WindowsHidDevice(deviceDefinition);
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

                    var deviceDefinition = GetDeviceDefinition(spDeviceInterfaceDetailData.DevicePath);

                    deviceDefinitions.Add(deviceDefinition);
                }

                APICalls.SetupDiDestroyDeviceInfoList(i);

                return deviceDefinitions;
            });
        }
        #endregion

        #region Private Static Methods
        protected override DeviceDefinition GetDeviceDefinition(string deviceId)
        {
            using (var safeFileHandle = APICalls.CreateFile(deviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero))
            {
                var hidCollectionCapabilities = new HidCollectionCapabilities();
                var hidAttributes = new HidAttributes();
                var pointerToPreParsedData = new IntPtr();
                var product = string.Empty;
                var serialNumber = string.Empty;
                var manufacturer = string.Empty;
                var pointerToBuffer = Marshal.AllocHGlobal(126);

                var preparsedDataResult = HidAPICalls.HidD_GetPreparsedData(safeFileHandle, ref pointerToPreParsedData);
                if (!preparsedDataResult)
                {
                    return null;
                }

                //TODO: Deal with issues here

                var getCapsResult = HidAPICalls.HidP_GetCaps(pointerToPreParsedData, ref hidCollectionCapabilities);

                //TODO: Deal with issues here

                if (!HidAPICalls.HidD_GetAttributes(safeFileHandle, ref hidAttributes))
                {
                    throw new Exception("Could not obtain attributes");
                }

                if (HidAPICalls.HidD_GetManufacturerString(safeFileHandle, pointerToBuffer, 126))
                {
                    manufacturer = Marshal.PtrToStringUni(pointerToBuffer);
                }

                if (HidAPICalls.HidD_GetSerialNumberString(safeFileHandle, pointerToBuffer, 126))
                {
                    serialNumber = Marshal.PtrToStringUni(pointerToBuffer);
                }

                if (HidAPICalls.HidD_GetProductString(safeFileHandle, pointerToBuffer, 126))
                {
                    product = Marshal.PtrToStringUni(pointerToBuffer);
                }

                Marshal.FreeHGlobal(pointerToBuffer);

                var getPreparsedDataResult = HidAPICalls.HidD_FreePreparsedData(ref pointerToPreParsedData);

                //TODO: Deal with issues here

                var deviceInformation = new WindowsHidDeviceDefinition
                {
                    DeviceId = deviceId,
                    //TODO Is this the right way around?
                    WriteBufferSize = hidCollectionCapabilities.InputReportByteLength,
                    ReadBufferSize = hidCollectionCapabilities.OutputReportByteLength,
                    Manufacturer = manufacturer,
                    Product = product,
                    ProductId = (ushort)hidAttributes.ProductId,
                    SerialNumber = serialNumber,
                    Usage = hidCollectionCapabilities.Usage,
                    UsagePage = hidCollectionCapabilities.UsagePage,
                    VendorId = (ushort)hidAttributes.VendorId,
                    VersionNumber = (ushort)hidAttributes.VersionNumber,
                    DeviceType = DeviceType.Hid
                };

                return deviceInformation;
            }
        }
        #endregion

        #region Public Static Methods
        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsHidDeviceFactory());
        }
        #endregion
    }
}
