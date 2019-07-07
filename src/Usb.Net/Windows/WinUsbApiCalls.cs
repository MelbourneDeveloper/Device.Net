using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Usb.Net.Windows
{
    public static partial class WinUsbApiCalls
    {
        #region Constants
        public const int EnglishLanguageID = 1033;
        public const uint DEVICE_SPEED = 1;
        public const byte USB_ENDPOINT_DIRECTION_MASK = 0X80;
        public const byte WritePipeId = 0x80;

        /// <summary>
        /// Not sure where this constant is defined...
        /// </summary>
        public const int DEFAULT_DESCRIPTOR_TYPE = 0x01;
        public const int USB_STRING_DESCRIPTOR_TYPE = 0x03;
        #endregion

        #region API Calls
        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, byte[] Buffer, uint BufferLength, ref uint LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool WinUsb_GetAssociatedInterface(SafeFileHandle InterfaceHandle, byte AssociatedInterfaceIndex, out SafeFileHandle AssociatedInterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_GetDescriptor(SafeFileHandle InterfaceHandle, byte DescriptorType, byte Index, ushort LanguageID, out USB_DEVICE_DESCRIPTOR deviceDesc, uint BufferLength, out uint LengthTransfered);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_GetDescriptor(SafeFileHandle InterfaceHandle, byte DescriptorType, byte Index, UInt16 LanguageID, byte[] Buffer, UInt32 BufferLength, out UInt32 LengthTransfered);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_Free(SafeFileHandle InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_Initialize(SafeFileHandle DeviceHandle, out SafeFileHandle InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_QueryDeviceInformation(IntPtr InterfaceHandle, uint InformationType, ref uint BufferLength, ref byte Buffer);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_QueryInterfaceSettings(SafeFileHandle InterfaceHandle, byte AlternateInterfaceNumber, out USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_QueryPipe(SafeFileHandle InterfaceHandle, byte AlternateInterfaceNumber, byte PipeIndex, out WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_ReadPipe(SafeFileHandle InterfaceHandle, byte PipeID, byte[] Buffer, uint BufferLength, out uint LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_SetPipePolicy(IntPtr InterfaceHandle, byte PipeID, uint PolicyType, uint ValueLength, ref uint Value);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_WritePipe(SafeFileHandle InterfaceHandle, byte PipeID, byte[] Buffer, uint BufferLength, out uint LengthTransferred, IntPtr Overlapped);
        #endregion

        #region Public Methods
        public static string GetDescriptor(SafeFileHandle defaultInterfaceHandle, byte index, string errorMessage)
        {
            var buffer = new byte[256];
            var isSuccess = WinUsb_GetDescriptor(defaultInterfaceHandle, USB_STRING_DESCRIPTOR_TYPE, index, EnglishLanguageID, buffer, (uint)buffer.Length, out var transfered);
            WindowsDeviceBase.HandleError(isSuccess, errorMessage);
            var descriptor = new string(Encoding.Unicode.GetChars(buffer, 2, (int)transfered));
            return descriptor.Substring(0, descriptor.Length - 1);
        }
        #endregion
    }
}
