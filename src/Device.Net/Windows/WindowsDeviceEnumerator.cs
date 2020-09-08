using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    public class WindowsDeviceEnumerator
    {
        private readonly ILogger Logger;
        private readonly GetClassGuid _getClassGuid;
        private readonly GetDeviceDefinition _getDeviceDefinition;
        private readonly FilterDeviceDefinition _filterDeviceDefinition;

        public WindowsDeviceEnumerator(
            ILogger logger,
            GetClassGuid getClassGuid,
            GetDeviceDefinition getDeviceDefinition,
            FilterDeviceDefinition filterDeviceDefinition
            )
        {
            Logger = logger;
            _getClassGuid = getClassGuid;
            _getDeviceDefinition = getDeviceDefinition;
            _filterDeviceDefinition = filterDeviceDefinition;
        }

        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
        {
            return await Task.Run<IEnumerable<ConnectedDeviceDefinition>>(() =>
            {
                IDisposable loggerScope = null;

                try
                {
                    Logger?.BeginScope("Filter Device Definition: {filterDeviceDefinition}", new object[] { _filterDeviceDefinition?.ToString() });

                    var deviceDefinitions = new Collection<ConnectedDeviceDefinition>();
                    var spDeviceInterfaceData = new SpDeviceInterfaceData();
                    var spDeviceInfoData = new SpDeviceInfoData();
                    var spDeviceInterfaceDetailData = new SpDeviceInterfaceDetailData();
                    spDeviceInterfaceData.CbSize = (uint)Marshal.SizeOf(spDeviceInterfaceData);
                    spDeviceInfoData.CbSize = (uint)Marshal.SizeOf(spDeviceInfoData);
                    string productIdHex = null;
                    string vendorHex = null;

                    var guidString = _getClassGuid().ToString();
                    var copyOfClassGuid = new Guid(guidString);
                    const int flags = APICalls.DigcfDeviceinterface | APICalls.DigcfPresent;

                    Logger?.LogDebug("About to call {call} for class Guid {guidString}. Flags: {flags}", nameof(APICalls.SetupDiGetClassDevs), guidString, flags);

                    var devicesHandle = APICalls.SetupDiGetClassDevs(ref copyOfClassGuid, IntPtr.Zero, IntPtr.Zero, flags);

                    spDeviceInterfaceDetailData.CbSize = IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize;

                    var i = -1;

                    if (_filterDeviceDefinition != null)
                    {
                        if (_filterDeviceDefinition.ProductId.HasValue) productIdHex = Helpers.GetHex(_filterDeviceDefinition.ProductId);
                        if (_filterDeviceDefinition.VendorId.HasValue) vendorHex = Helpers.GetHex(_filterDeviceDefinition.VendorId);
                    }

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
                                    Logger?.LogDebug("The call to " + nameof(APICalls.SetupDiEnumDeviceInterfaces) + "  returned ERROR_NO_MORE_ITEMS");
                                    break;
                                }

                                if (errorCode > 0)
                                {
                                    Logger?.LogWarning("{call} called successfully but a device was skipped while enumerating because something went wrong. The device was at index {index}. The error code was {errorCode}.", nameof(APICalls.SetupDiEnumDeviceInterfaces), i, errorCode);
                                }
                            }

                            isSuccess = APICalls.SetupDiGetDeviceInterfaceDetail(devicesHandle, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);
                            if (!isSuccess)
                            {
                                var errorCode = Marshal.GetLastWin32Error();

                                if (errorCode == APICalls.ERROR_NO_MORE_ITEMS)
                                {
                                    Logger?.LogDebug("The call to {call} returned ERROR_NO_MORE_ITEMS", new object[] { nameof(APICalls.SetupDiEnumDeviceInterfaces) });
                                    //TODO: This probably can't happen but leaving this here because there was some strange behaviour
                                    break;
                                }

                                if (errorCode > 0)
                                {
                                    Logger?.LogWarning("{nameof(APICalls.SetupDiGetDeviceInterfaceDetail)} called successfully but a device was skipped while enumerating because something went wrong. The device was at index {i}. The error code was {errorCode}.", nameof(APICalls.SetupDiEnumDeviceInterfaces), i, errorCode);
                                }
                            }

                            //Note this is a bit nasty but we can filter Vid and Pid this way I think...
                            if (_filterDeviceDefinition != null)
                            {
                                if (_filterDeviceDefinition.VendorId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ContainsIgnoreCase(vendorHex)) continue;
                                if (_filterDeviceDefinition.ProductId.HasValue && !spDeviceInterfaceDetailData.DevicePath.ContainsIgnoreCase(productIdHex)) continue;
                            }

                            var connectedDeviceDefinition = _getDeviceDefinition(spDeviceInterfaceDetailData.DevicePath);

                            if (connectedDeviceDefinition == null)
                            {
                                Logger?.LogWarning("Device with path {devicePath} was skipped. Area: {area} See previous logs.", spDeviceInterfaceDetailData.DevicePath, GetType().Name);
                                continue;
                            }

                            if (!DeviceManager.IsDefinitionMatch(_filterDeviceDefinition, connectedDeviceDefinition)) continue;

                            deviceDefinitions.Add(connectedDeviceDefinition);
                        }
#pragma warning disable CA1031
                        catch (Exception ex)
                        {
                            //Log and move on
                            Logger?.LogError(ex, ex.Message);
                        }
#pragma warning restore CA1031
                    }

                    APICalls.SetupDiDestroyDeviceInfoList(devicesHandle);

                    return deviceDefinitions;
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error calling " + nameof(GetConnectedDeviceDefinitionsAsync));
                    throw;
                }
                finally
                {
                    loggerScope?.Dispose();
                }
            });
        }
    }
}
