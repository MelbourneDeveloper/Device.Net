using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    public delegate Task<bool> IsMatch(ConnectedDeviceDefinition connectedDeviceDefinition);

    public class WindowsDeviceEnumerator
    {
        private readonly ILogger Logger;
        private readonly Guid _classGuid;
        private readonly GetDeviceDefinition _getDeviceDefinition;
        private readonly IsMatch _isMatch;

        public WindowsDeviceEnumerator(
            ILogger logger,
            Guid classGuid,
            GetDeviceDefinition getDeviceDefinition,
            IsMatch isMatch
            )
        {
            Logger = logger ?? NullLogger.Instance;
            _classGuid = classGuid;
            _getDeviceDefinition = getDeviceDefinition;
            _isMatch = isMatch;
        }

        public async Task<IReadOnlyCollection<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync()
        {
            return await Task.Run<IReadOnlyCollection<ConnectedDeviceDefinition>>(async () =>
            {
                IDisposable loggerScope = null;

                try
                {
                    loggerScope = Logger.BeginScope("Calling " + nameof(GetConnectedDeviceDefinitionsAsync));

                    var deviceDefinitions = new List<ConnectedDeviceDefinition>();
                    var spDeviceInterfaceData = new SpDeviceInterfaceData();
                    var spDeviceInfoData = new SpDeviceInfoData();
                    var spDeviceInterfaceDetailData = new SpDeviceInterfaceDetailData();
                    spDeviceInterfaceData.CbSize = (uint)Marshal.SizeOf(spDeviceInterfaceData);
                    spDeviceInfoData.CbSize = (uint)Marshal.SizeOf(spDeviceInfoData);

                    const int flags = APICalls.DigcfDeviceinterface | APICalls.DigcfPresent;

                    var copyOfClassGuid = new Guid(_classGuid.ToString());

                    Logger.LogDebug("About to call {call} for class Guid {guidString}. Flags: {flags}", nameof(APICalls.SetupDiGetClassDevs), _classGuid.ToString(), flags);

                    var devicesHandle = APICalls.SetupDiGetClassDevs(ref copyOfClassGuid, IntPtr.Zero, IntPtr.Zero, flags);

                    spDeviceInterfaceDetailData.CbSize = IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize;

                    var i = -1;

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
                                    Logger.LogDebug("The call to " + nameof(APICalls.SetupDiEnumDeviceInterfaces) + "  returned ERROR_NO_MORE_ITEMS");
                                    break;
                                }

                                if (errorCode > 0)
                                {
                                    Logger.LogWarning("{call} called successfully but a device was skipped while enumerating because something went wrong. The device was at index {index}. The error code was {errorCode}.", nameof(APICalls.SetupDiEnumDeviceInterfaces), i, errorCode);
                                }
                            }

                            isSuccess = APICalls.SetupDiGetDeviceInterfaceDetail(devicesHandle, ref spDeviceInterfaceData, ref spDeviceInterfaceDetailData, 256, out _, ref spDeviceInfoData);
                            if (!isSuccess)
                            {
                                var errorCode = Marshal.GetLastWin32Error();

                                if (errorCode == APICalls.ERROR_NO_MORE_ITEMS)
                                {
                                    Logger.LogDebug("The call to {call} returned ERROR_NO_MORE_ITEMS", new object[] { nameof(APICalls.SetupDiEnumDeviceInterfaces) });
                                    //TODO: This probably can't happen but leaving this here because there was some strange behaviour
                                    break;
                                }

                                if (errorCode > 0)
                                {
                                    Logger.LogWarning("{nameof(APICalls.SetupDiGetDeviceInterfaceDetail)} called successfully but a device was skipped while enumerating because something went wrong. The device was at index {i}. The error code was {errorCode}.", nameof(APICalls.SetupDiEnumDeviceInterfaces), i, errorCode);
                                }
                            }

                            var connectedDeviceDefinition = _getDeviceDefinition(spDeviceInterfaceDetailData.DevicePath);

                            if (connectedDeviceDefinition == null)
                            {
                                Logger.LogWarning("Device with path {devicePath} was skipped. Area: {area} See previous logs.", spDeviceInterfaceDetailData.DevicePath, GetType().Name);
                                continue;
                            }

                            if (!await _isMatch(connectedDeviceDefinition)) continue;

                            deviceDefinitions.Add(connectedDeviceDefinition);
                        }
#pragma warning disable CA1031
                        catch (Exception ex)
                        {
                            //Log and move on
                            Logger.LogError(ex, ex.Message);
                        }
#pragma warning restore CA1031
                    }

                    APICalls.SetupDiDestroyDeviceInfoList(devicesHandle);

                    return new ReadOnlyCollection<ConnectedDeviceDefinition>(deviceDefinitions);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error calling " + nameof(GetConnectedDeviceDefinitionsAsync));
                    throw;
                }
                finally
                {
                    loggerScope.Dispose();
                }
            });
        }
    }
}
