using Device.Net;
using Device.Net.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public class WindowsHidDeviceFactory : IDeviceFactory
    {
        public DeviceType DeviceType => DeviceType.Hid;

        public static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new WindowsHidDeviceFactory());
        }

        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceType == DeviceType.Usb) return null;
            return new WindowsHidDevice(deviceDefinition);
        }

        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            return await Task.Run<IEnumerable<DeviceDefinition>>(() =>
            {
                var deviceInformations = new Collection<DeviceDefinition>();
                var spDeviceInterfaceData = new SpDeviceInterfaceData();
                var spDeviceInfoData = new SpDeviceInfoData();
                var spDeviceInterfaceDetailData = new SpDeviceInterfaceDetailData();
                spDeviceInterfaceData.CbSize = (uint)Marshal.SizeOf(spDeviceInterfaceData);
                spDeviceInfoData.CbSize = (uint)Marshal.SizeOf(spDeviceInfoData);

                var classGuid = WindowsDeviceConstants.GUID_DEVINTERFACE_HID;
                //Split this method up for Usb devices and move this down a library
                //var classGuid = deviceType == DeviceType.Hid ? WindowsDeviceConstants.GUID_DEVINTERFACE_HID : WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE;

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

                    //Note this is a bit nast but we can filter Vid and Pid this way I think...
                    var vendorHex = vendorId?.ToString("X").ToLower().PadLeft(4, '0');
                    var productIdHex = productId?.ToString("X").ToLower().PadLeft(4, '0');
                    if (vendorId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(vendorHex)) continue;
                    if (productId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ToLower().Contains(productIdHex)) continue;

                    deviceInformations.Add(GetDeviceInformation(spDeviceInterfaceDetailData.DevicePath));
                }

                APICalls.SetupDiDestroyDeviceInfoList(i);

                return deviceInformations;
            });
        }

        private static WindowsHidDeviceInformation GetDeviceInformation(string devicePath)
        {
            using (var safeFileHandle = APICalls.CreateFile(devicePath, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero))
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

                var deviceInformation = new WindowsHidDeviceInformation
                {
                    DeviceId = devicePath,
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
                    VersionNumber = (ushort)hidAttributes.VersionNumber
                };

                return deviceInformation;
            }
        }
    }
}
