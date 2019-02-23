using Device.Net.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Device.Net.UWP
{
    /// <summary>
    /// TODO: Merge this factory class with other factory classes. I.e. create a DeviceFactoryBase class
    /// </summary>
    public abstract class UWPDeviceFactoryBase
    {
        #region Fields
        //TODO: Should we allow enumerating devices that are defined but not connected? This is very good for situations where we need the Id of the device before it is physically connected.
        protected const string InterfaceEnabledPart = "AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
        #endregion

        #region Protected Abstraction Properties
        protected abstract string VendorFilterName { get; }
        protected abstract string ProductFilterName { get; }
        #endregion

        #region Public Properties
        public ILogger Logger { get; set; }
        #endregion

        #region Public Abstract Properties
        public abstract DeviceType DeviceType { get; }
        #endregion

        #region Protected Abstract Methods
        protected abstract string GetAqsFilter(uint? vendorId, uint? productId);
        #endregion

        #region Abstraction Methods
        protected string GetVendorPart(uint? vendorId)
        {
            string vendorPart = null;
            if (vendorId.HasValue) vendorPart = $"AND {VendorFilterName}:={vendorId.Value}";
            return vendorPart;
        }

        protected string GetProductPart(uint? productId)
        {
            string productPart = null;
            if (productId.HasValue) productPart = $"AND {ProductFilterName}:={productId.Value}";
            return productPart;
        }
        #endregion

        #region Protected Methods
        protected void Log(string message, Exception ex)
        {
            var callerMemberName = "";
            Logger?.Log(message, $"{ nameof(UWPDeviceFactoryBase)} - {callerMemberName}", ex, ex != null ? LogLevel.Error : LogLevel.Information);
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            var aqsFilter = GetAqsFilter(deviceDefinition.VendorId, deviceDefinition.ProductId);

            var deviceInformationCollection = await wde.DeviceInformation.FindAllAsync(aqsFilter).AsTask();

            var deviceDefinitions = deviceInformationCollection.Select(d => GetDeviceInformation(d, DeviceType));

            var deviceDefinitionList = new List<ConnectedDeviceDefinition>();

            foreach (var deviceDef in deviceDefinitions)
            {
                var connectionInformation = await TestConnection(deviceDef.DeviceId);
                if (connectionInformation.CanConnect)
                {
                    deviceDef.UsagePage = connectionInformation.UsagePage;

                    deviceDefinitionList.Add(deviceDef);
                }
            }

            return deviceDefinitionList;
        }
        #endregion

        #region Public Abstract Methods
        /// <summary>
        /// Some devices display as being enable but still cannot be connected to, so run a test to make sure they can be connected before returning the definition
        /// </summary>
        public abstract Task<ConnectionInfo> TestConnection(string deviceId);
        #endregion

        #region Public Static Methods
        public static ConnectedDeviceDefinition GetDeviceInformation(wde.DeviceInformation deviceInformation, DeviceType deviceType)
        {
            var retVal = WindowsDeviceFactoryBase.GetDeviceDefinitionFromWindowsDeviceId(deviceInformation.Id, deviceType);

            //foreach (var keyValuePair in deviceInformation.Properties)
            //{
            //    if (keyValuePair.Key == ProductNamePropertyName) retVal.ProductName = (string)keyValuePair.Value;
            //    System.Diagnostics.Debug.WriteLine($"{keyValuePair.Key} {keyValuePair.Value}");
            //}

            retVal.ProductName = deviceInformation.Name;

            return retVal;
        }
        #endregion
    }
}
