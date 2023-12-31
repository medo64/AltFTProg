namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Xml;

/// <summary>
/// FTDI XML template.
/// </summary>
public class FtdiXmlTemplate {

    private FtdiXmlTemplate(FtdiDeviceType deviceType, ReadOnlyCollection<KeyValuePair<string, string>> properties) {
        DeviceType = deviceType;
        Properties = properties;
    }


    /// <summary>
    /// Gets the device type this template is for.
    /// </summary>
    public FtdiDeviceType DeviceType { get; }

    /// <summary>
    /// Gets raw properties from the XML template.
    /// This is a simplified version of the XML parsing.
    /// </summary>
    public ReadOnlyCollection<KeyValuePair<string, string>> Properties { get; }

    /// <summary>
    /// Applies XML template to the specified device.
    /// </summary>
    /// <param name="device">Device.</param>
    /// <exception cref="InvalidOperationException">Unsupported device.</exception>
    public void Apply(FtdiDevice device) {
        var deviceCommon = device as FtdiCommonDevice;
        var device232R = device as Ftdi232RDevice;
        var deviceXSeries = device as FtdiXSeriesDevice;

        var newManufacturer = deviceCommon?.Manufacturer ?? "";
        var newProductDescription = deviceCommon?.ProductDescription ?? "";
        var newSerialNumber = deviceCommon?.SerialNumber ?? "";
        var newSerialNumberPrefix = "";
        var newSerialNumberGenerate = false;
        var newSerialNumberEnabled = deviceCommon?.SerialNumberEnabled ?? (newSerialNumber.Length > 0);

        foreach (var property in Properties) {
            var name = property.Key;
            var value = property.Value;
            Helpers.WriteDebug($"XML property {name} = {value}");

            switch (name) {
                case "USB_Device_Descriptor/VID_PID": // ignoring
                    break;

                case "USB_Device_Descriptor/idVendor": {
                        var newVendorId = ushort.Parse(value, NumberStyles.HexNumber);
                        if ((deviceCommon != null) && (deviceCommon.VendorId != newVendorId)) {
                            Helpers.WriteDebug($"  VendorId = {newVendorId}");
                            deviceCommon.VendorId = newVendorId;
                        }
                    }
                    break;

                case "USB_Device_Descriptor/idProduct": {
                        var newProductId = ushort.Parse(value, NumberStyles.HexNumber);
                        if ((deviceCommon != null) && (deviceCommon!.ProductId != newProductId)) {
                            Helpers.WriteDebug($"  VendorId = {newProductId}");
                            deviceCommon.ProductId = newProductId;
                        }
                    }
                    break;

                case "USB_Device_Descriptor/bcdUSB": // ignore it
                    break;

                case "bmAttributes/RemoteWakeupEnabled": {
                        var newRemoteWakeupEnabled = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.RemoteWakeupEnabled != newRemoteWakeupEnabled)) {
                            Helpers.WriteDebug($"  RemoteWakeupEnabled = {newRemoteWakeupEnabled}");
                            deviceCommon.RemoteWakeupEnabled = newRemoteWakeupEnabled;
                        }
                    }
                    break;

                case "bmAttributes/SelfPowered": {
                        var newSelfPowered = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.SelfPowered != newSelfPowered)) {
                            Helpers.WriteDebug($"  SelfPowered = {newSelfPowered}");
                            deviceCommon.SelfPowered = newSelfPowered;
                        }
                    }
                    break;

                case "bmAttributes/BusPowered": {
                        var newBusPowered = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.BusPowered != newBusPowered)) {
                            Helpers.WriteDebug($"  BusPowered = {newBusPowered}");
                            deviceCommon.BusPowered = newBusPowered;
                        }
                    }
                    break;

                case "USB_Config_Descriptor/IOpullDown": {
                        var newPulldownPinsInSuspend = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.PulldownPinsInSuspend = newPulldownPinsInSuspend)) {
                            Helpers.WriteDebug($"  PulldownPinsInSuspend = {newPulldownPinsInSuspend}");
                            deviceCommon.PulldownPinsInSuspend = newPulldownPinsInSuspend;
                        }
                    }
                    break;

                case "USB_Config_Descriptor/MaxPower": {
                        var newMaxBusPower = int.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.MaxBusPower != newMaxBusPower)) {
                            Helpers.WriteDebug($"  MaxBusPower = {newMaxBusPower}");
                            deviceCommon.MaxBusPower = newMaxBusPower;
                        }
                    }
                    break;

                case "USB_String_Descriptors/Manufacturer": {
                        newManufacturer = value.Trim();
                    }
                    break;

                case "USB_String_Descriptors/Product_Description": {
                        newProductDescription = value.Trim();
                    }
                    break;

                case "USB_String_Descriptors/SerialNumber_Enabled": {
                        newSerialNumberEnabled = bool.Parse(value);
                    }
                    break;

                case "USB_String_Descriptors/SerialNumber": {
                        newSerialNumber = value.Trim();
                    }
                    break;

                case "USB_String_Descriptors/SerialNumberPrefix": {
                        newSerialNumberPrefix = value.Trim();
                    }
                    break;

                case "USB_String_Descriptors/SerialNumber_AutoGenerate": {
                        if (bool.Parse(value)) {
                            newSerialNumberGenerate = true;
                        }
                    }
                    break;

                case "Hardware_Specific/HighIO": {
                        var newHighCurrentIO = bool.Parse(value);
                        if ((deviceCommon != null) && (device232R.HighCurrentIO != newHighCurrentIO)) {
                            Helpers.WriteDebug($"  HighCurrentIO = {newHighCurrentIO}");
                            device232R.HighCurrentIO = newHighCurrentIO;
                        }
                    }
                    break;

                case "Hardware_Specific/D2XX":
                case "Driver/D2XX": {
                        var newD2xxDirectDriver = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.D2xxDirectDriver != newD2xxDirectDriver)) {
                            Helpers.WriteDebug($"  D2xxDirectDriver = {newD2xxDirectDriver}");
                            deviceCommon.D2xxDirectDriver = newD2xxDirectDriver;
                        }
                    }
                    break;

                case "Driver/VCP": {
                        var newVirtualComPortDriver = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceXSeries.VirtualComPortDriver != newVirtualComPortDriver)) {
                            Helpers.WriteDebug($"  VirtualComPortDriver = {newVirtualComPortDriver}");
                            deviceXSeries.VirtualComPortDriver = newVirtualComPortDriver;
                        }
                    }
                    break;

                case "Hardware_Specific/ExternalOscillator": {
                        var newExternalOscillator = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.ExternalOscillator != newExternalOscillator)) {
                            Helpers.WriteDebug($"  ExternalOscillator = {newExternalOscillator}");
                            deviceXSeries.ExternalOscillator = newExternalOscillator;
                        }
                    }
                    break;

                case "Hardware_Specific/USB_Suspend_VBus":
                    //ignore
                    break;

                case "Hardware_Specific/RS485_Echo_Suppress": {
                        var newRs485EchoSuppression = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceXSeries.Rs485EchoSuppression != newRs485EchoSuppression)) {
                            Helpers.WriteDebug($"  Rs485EchoSuppression = {newRs485EchoSuppression}");
                            deviceXSeries.Rs485EchoSuppression = newRs485EchoSuppression;
                        }
                    }
                    break;

                case "Hardware/Device":
                    //ignore
                    break;

                case "Invert_RS232_Signals/TXD": {
                        var newTxdInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.TxdInverted != newTxdInverted)) {
                            Helpers.WriteDebug($"  TxdInverted = {newTxdInverted}");
                            deviceCommon.TxdInverted = newTxdInverted;
                        }
                    }
                    break;

                case "Invert_RS232_Signals/RXD": {
                        var newRxdInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.RxdInverted != newRxdInverted)) {
                            Helpers.WriteDebug($"  RxdInverted = {newRxdInverted}");
                            deviceCommon.RxdInverted = newRxdInverted;
                        }
                    }
                    break;

                case "Invert_RS232_Signals/RTS": {
                        var newRtsInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.RtsInverted != newRtsInverted)) {
                            Helpers.WriteDebug($"  RtsInverted = {newRtsInverted}");
                            deviceCommon.RtsInverted = newRtsInverted;
                        }
                    }
                    break;

                case "Invert_RS232_Signals/CTS": {
                        var newCtsInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.CtsInverted != newCtsInverted)) {
                            Helpers.WriteDebug($"  CtsInverted = {newCtsInverted}");
                            deviceCommon.CtsInverted = newCtsInverted;
                        }
                    }
                    break;

                case "Invert_RS232_Signals/DTR": {
                        var newDtrInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.DtrInverted != newDtrInverted)) {
                            Helpers.WriteDebug($"  DtrInverted = {newDtrInverted}");
                            deviceCommon.DtrInverted = newDtrInverted;
                        }
                    }
                    break;

                case "Invert_RS232_Signals/DSR": {
                        var newDsrInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.DsrInverted != newDsrInverted)) {
                            Helpers.WriteDebug($"  DsrInverted = {newDsrInverted}");
                            deviceCommon.DsrInverted = newDsrInverted;
                        }
                    }
                    break;

                case "Invert_RS232_Signals/DCD": {
                        var newDcdInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.DcdInverted != newDcdInverted)) {
                            Helpers.WriteDebug($"  DcdInverted = {newDcdInverted}");
                            deviceCommon.DcdInverted = newDcdInverted;
                        }
                    }
                    break;

                case "Invert_RS232_Signals/RI": {
                        var newRiInverted = bool.Parse(value);
                        if ((deviceCommon != null) && (deviceCommon.RiInverted != newRiInverted)) {
                            Helpers.WriteDebug($"  RiInverted = {newRiInverted}");
                            deviceCommon.RiInverted = newRiInverted;
                        }
                    }
                    break;

                case "IO_Controls/C0": {
                        var newCBus0Signal = FtdiXmlTemplate.ParseCBus0FT232Signal(value);
                        if ((device232R != null) && (device232R.CBus0Signal != newCBus0Signal)) {
                            Helpers.WriteDebug($"  CBus0Signal = {newCBus0Signal}");
                            device232R.CBus0Signal = newCBus0Signal;
                        }
                    }
                    break;

                case "IO_Controls/C1": {
                        var newCBus1Signal = ParseCBus1FT232Signal(value);
                        if ((device232R != null) && (device232R.CBus1Signal != newCBus1Signal)) {
                            Helpers.WriteDebug($"  CBus1Signal = {newCBus1Signal}");
                            device232R.CBus1Signal = newCBus1Signal;
                        }
                    }
                    break;

                case "IO_Controls/C2": {
                        var newCBus2Signal = ParseCBus2FT232Signal(value);
                        if ((device232R != null) && (device232R.CBus2Signal != newCBus2Signal)) {
                            Helpers.WriteDebug($"  CBus2Signal = {newCBus2Signal}");
                            device232R.CBus2Signal = newCBus2Signal;
                        }
                    }
                    break;

                case "IO_Controls/C3": {
                        var newCBus3Signal = ParseCBus3FT232Signal(value);
                        if ((device232R != null) && (device232R.CBus3Signal != newCBus3Signal)) {
                            Helpers.WriteDebug($"  CBus3Signal = {newCBus3Signal}");
                            device232R.CBus3Signal = newCBus3Signal;
                        }
                    }
                    break;

                case "IO_Controls/C4": {
                        var newCBus4Signal = ParseCBus4FT232Signal(value);
                        if ((device232R != null) && (device232R.CBus4Signal != newCBus4Signal)) {
                            Helpers.WriteDebug($"  CBus4Signal = {newCBus4Signal}");
                            device232R.CBus4Signal = newCBus4Signal;
                        }
                    }
                    break;

                case "CBUS_Signals/C0": {
                        var newCBus0Signal = ParseCBus4FTXSeriesSignal(value);
                        if ((deviceXSeries != null) && (deviceXSeries.CBus0Signal != newCBus0Signal)) {
                            Helpers.WriteDebug($"  CBus0Signal = {newCBus0Signal}");
                            deviceXSeries.CBus0Signal = newCBus0Signal;
                        }
                    }
                    break;

                case "CBUS_Signals/C1": {
                        var newCBus1Signal = ParseCBus4FTXSeriesSignal(value);
                        if ((deviceXSeries != null) && (deviceXSeries.CBus1Signal != newCBus1Signal)) {
                            Helpers.WriteDebug($"  CBus1Signal = {newCBus1Signal}");
                            deviceXSeries.CBus1Signal = newCBus1Signal;
                        }
                    }
                    break;

                case "CBUS_Signals/C2": {
                        var newCBus2Signal = ParseCBus4FTXSeriesSignal(value);
                        if ((deviceXSeries != null) && (deviceXSeries.CBus2Signal != newCBus2Signal)) {
                            Helpers.WriteDebug($"  CBus2Signal = {newCBus2Signal}");
                            deviceXSeries.CBus2Signal = newCBus2Signal;
                        }
                    }
                    break;

                case "CBUS_Signals/C3": {
                        var newCBus3Signal = FtdiXmlTemplate.ParseCBus4FTXSeriesSignal(value);
                        if ((deviceXSeries != null) && (deviceXSeries.CBus3Signal != newCBus3Signal)) {
                            Helpers.WriteDebug($"  CBus3Signal = {newCBus3Signal}");
                            deviceXSeries.CBus3Signal = newCBus3Signal;
                        }
                    }
                    break;

                case "Battery_Charge_Detect/Enable": {
                        var newBatteryChargeEnable = bool.Parse(value);
                        if ((deviceXSeries != null) && (deviceXSeries.BatteryChargeEnable != newBatteryChargeEnable)) {
                            Helpers.WriteDebug($"  BatteryChargeEnable = {newBatteryChargeEnable}");
                            deviceXSeries.BatteryChargeEnable = newBatteryChargeEnable;
                        }
                    }
                    break;

                case "Battery_Charge_Detect/Power_Enable": {
                        var newForcePowerEnable = bool.Parse(value);
                        if ((deviceXSeries != null) && (deviceXSeries.ForcePowerEnable != newForcePowerEnable)) {
                            Helpers.WriteDebug($"  ForcePowerEnable = {newForcePowerEnable}");
                            deviceXSeries.ForcePowerEnable = newForcePowerEnable;
                        }
                    }
                    break;

                case "Battery_Charge_Detect/Deactivate_Sleep": {
                        var newDeactivateSleep = bool.Parse(value);
                        if ((deviceXSeries != null) && (deviceXSeries.DeactivateSleep != newDeactivateSleep)) {
                            Helpers.WriteDebug($"  DeactivateSleep = {newDeactivateSleep}");
                            deviceXSeries.DeactivateSleep = newDeactivateSleep;
                        }
                    }
                    break;

                case "DBUS/SlowSlew": {
                        var newDBusSlowSlew = bool.Parse(value);
                        if ((deviceXSeries != null) && (deviceXSeries.DBusSlowSlew != newDBusSlowSlew)) {
                            Helpers.WriteDebug($"  DBusSlowSlew = {newDBusSlowSlew}");
                            deviceXSeries.DBusSlowSlew = newDBusSlowSlew;
                        }
                    }
                    break;

                case "DBUS/Drive": {
                        var newDBusDriveCurrent = int.Parse(value.Replace("mA", ""), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        if ((deviceXSeries != null) && (deviceXSeries.DBusDriveCurrent != newDBusDriveCurrent)) {
                            Helpers.WriteDebug($"  DBusDriveCurrent = {newDBusDriveCurrent}");
                            deviceXSeries.DBusDriveCurrent = newDBusDriveCurrent;
                        }
                    }
                    break;

                case "DBUS/Schmitt": {
                        var newDBusSchmittInput = bool.Parse(value);
                        if ((deviceXSeries != null) && (deviceXSeries.DBusSchmittInput != newDBusSchmittInput)) {
                            Helpers.WriteDebug($"  DBusSchmittInput = {newDBusSchmittInput}");
                            deviceXSeries.DBusSchmittInput = newDBusSchmittInput;
                        }
                    }
                    break;

                case "CBUS/SlowSlew": {
                        var newCBusSlowSlew = bool.Parse(value);
                        if ((deviceXSeries != null) && (deviceXSeries.CBusSlowSlew != newCBusSlowSlew)) {
                            Helpers.WriteDebug($"  CBusSlowSlew = {newCBusSlowSlew}");
                            deviceXSeries.CBusSlowSlew = newCBusSlowSlew;
                        }
                    }
                    break;

                case "CBUS/Drive": {
                        var newCBusDriveCurrent = int.Parse(value.Replace("mA", ""), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        if ((deviceXSeries != null) && (deviceXSeries.CBusDriveCurrent != newCBusDriveCurrent)) {
                            Helpers.WriteDebug($"  CBusDriveCurrent = {newCBusDriveCurrent}");
                            deviceXSeries.CBusDriveCurrent = newCBusDriveCurrent;
                        }
                    }
                    break;

                case "CBUS/Schmitt": {
                        var newCBusSchmittInput = bool.Parse(value);
                        if ((deviceXSeries != null) && (deviceXSeries.CBusSchmittInput != newCBusSchmittInput)) {
                            Helpers.WriteDebug($"  CBusSchmittInput = {newCBusSchmittInput}");
                            deviceXSeries.CBusSchmittInput = newCBusSchmittInput;
                        }
                    }
                    break;

                default: Helpers.WriteDebug($"  Unknown property"); break;
            }
        }

        if (newSerialNumberGenerate) { newSerialNumber = FtdiDevice.GetRandomSerialNumber(newSerialNumberPrefix, 6); }

        if ((deviceCommon != null)
            && (!deviceCommon.Manufacturer.Equals(newManufacturer)
             || !deviceCommon.ProductDescription.Equals(newProductDescription)
             || !deviceCommon.SerialNumber.Equals(newSerialNumber))) {
            Helpers.WriteDebug($"  Manufacturer = {newManufacturer}");
            Helpers.WriteDebug($"  ProductDescription = {newProductDescription}");
            Helpers.WriteDebug($"  SerialNumber = {newSerialNumber}");
            deviceCommon.SetStringDescriptors(newManufacturer, newProductDescription, newSerialNumber);
            deviceCommon.SerialNumberEnabled = newSerialNumberEnabled && (deviceCommon.SerialNumber.Length > 0);
        }
    }


    /// <summary>
    /// Loads the XML template from the specified stream.
    /// Quite simplified in the terms of XML parsing.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <exception cref="InvalidOperationException">Unrecognized chip type in XML template.</exception>
    public static FtdiXmlTemplate Load(Stream stream) {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(stream);

        var chipType = default(string?);

        var properties = new List<KeyValuePair<string, string>>();
        TraverseNodes(xmlDoc.DocumentElement, properties);
        for (var i = 0; i < properties.Count; i++) {
            if (properties[i].Key.Equals("Chip_Details/Type", StringComparison.OrdinalIgnoreCase)) {
                chipType = properties[i].Value;
                properties.RemoveAt(i);
                break;
            }
        }

        var deviceType = chipType switch {
            "FT232R" => FtdiDeviceType.FT232R,
            "FT X Series" => FtdiDeviceType.FTXSeries,
            _ => throw new InvalidOperationException("Unrecognized chip type in XML template (" + chipType + ")")
        };

        return new FtdiXmlTemplate(deviceType, properties.AsReadOnly());
    }


    private static void TraverseNodes(XmlNode? node, List<KeyValuePair<string, string>> properties) {
        if (node == null) { return; }
        if (node.NodeType == XmlNodeType.Text) {
            if (node.ParentNode == null) { return; }
            var parentName = node.ParentNode.ParentNode?.Name ?? "";
            var name = node.ParentNode.Name;
            var value = node.Value;
            if (string.IsNullOrEmpty(value)) { return; }
            properties.Add(new KeyValuePair<string, string>(parentName + "/" + name, value));
        }

        foreach (XmlNode child in node.ChildNodes) {
            TraverseNodes(child, properties);
        }
    }


    private static Ftdi232RDevice.CBus0PinSignal ParseCBus0FT232Signal(string text) {
        return text switch {
            "TXDEN" => Ftdi232RDevice.CBus0PinSignal.TxdEnable,
            "PWREN#" => Ftdi232RDevice.CBus0PinSignal.PowerEnable,
            "RXLED#" => Ftdi232RDevice.CBus0PinSignal.RxLed,
            "TXLED#" => Ftdi232RDevice.CBus0PinSignal.TxLed,
            "TX&RXLED#" => Ftdi232RDevice.CBus0PinSignal.TxRxLed,
            "SLEEP#" => Ftdi232RDevice.CBus0PinSignal.Sleep,
            "CLK48" => Ftdi232RDevice.CBus0PinSignal.Clock48Mhz,
            "CLK24" => Ftdi232RDevice.CBus0PinSignal.Clock24Mhz,
            "CLK12" => Ftdi232RDevice.CBus0PinSignal.Clock12Mhz,
            "CLK6" => Ftdi232RDevice.CBus0PinSignal.Clock6Mhz,
            "I/O MODE" => Ftdi232RDevice.CBus0PinSignal.IOMode,
            "BitBang WR#" => Ftdi232RDevice.CBus0PinSignal.BitBangWr,
            "BitBang RD#" => Ftdi232RDevice.CBus0PinSignal.BitBangRd,
            "RXF#" => Ftdi232RDevice.CBus0PinSignal.RxF,
            _ => throw new InvalidOperationException($"Unknown CBus signal: {text}"),
        };
    }

    private static Ftdi232RDevice.CBus1PinSignal ParseCBus1FT232Signal(string text) {
        return text switch {
            "TXDEN" => Ftdi232RDevice.CBus1PinSignal.TxdEnable,
            "PWREN#" => Ftdi232RDevice.CBus1PinSignal.PowerEnable,
            "RXLED#" => Ftdi232RDevice.CBus1PinSignal.RxLed,
            "TXLED#" => Ftdi232RDevice.CBus1PinSignal.TxLed,
            "TX&RXLED#" => Ftdi232RDevice.CBus1PinSignal.TxRxLed,
            "SLEEP#" => Ftdi232RDevice.CBus1PinSignal.Sleep,
            "CLK48" => Ftdi232RDevice.CBus1PinSignal.Clock48Mhz,
            "CLK24" => Ftdi232RDevice.CBus1PinSignal.Clock24Mhz,
            "CLK12" => Ftdi232RDevice.CBus1PinSignal.Clock12Mhz,
            "CLK6" => Ftdi232RDevice.CBus1PinSignal.Clock6Mhz,
            "I/O MODE" => Ftdi232RDevice.CBus1PinSignal.IOMode,
            "BitBang WR#" => Ftdi232RDevice.CBus1PinSignal.BitBangWr,
            "BitBang RD#" => Ftdi232RDevice.CBus1PinSignal.BitBangRd,
            "TXE#" => Ftdi232RDevice.CBus1PinSignal.TxE,
            _ => throw new InvalidOperationException($"Unknown CBus signal: {text}"),
        };
    }

    private static Ftdi232RDevice.CBus2PinSignal ParseCBus2FT232Signal(string text) {
        return text switch {
            "TXDEN" => Ftdi232RDevice.CBus2PinSignal.TxdEnable,
            "PWREN#" => Ftdi232RDevice.CBus2PinSignal.PowerEnable,
            "RXLED#" => Ftdi232RDevice.CBus2PinSignal.RxLed,
            "TXLED#" => Ftdi232RDevice.CBus2PinSignal.TxLed,
            "TX&RXLED#" => Ftdi232RDevice.CBus2PinSignal.TxRxLed,
            "SLEEP#" => Ftdi232RDevice.CBus2PinSignal.Sleep,
            "CLK48" => Ftdi232RDevice.CBus2PinSignal.Clock48Mhz,
            "CLK24" => Ftdi232RDevice.CBus2PinSignal.Clock24Mhz,
            "CLK12" => Ftdi232RDevice.CBus2PinSignal.Clock12Mhz,
            "CLK6" => Ftdi232RDevice.CBus2PinSignal.Clock6Mhz,
            "I/O MODE" => Ftdi232RDevice.CBus2PinSignal.IOMode,
            "BitBang WR#" => Ftdi232RDevice.CBus2PinSignal.BitBangWr,
            "BitBang RD#" => Ftdi232RDevice.CBus2PinSignal.BitBangRd,
            "RD#" => Ftdi232RDevice.CBus2PinSignal.Rd,
            _ => throw new InvalidOperationException($"Unknown CBus signal: {text}"),
        };
    }

    private static Ftdi232RDevice.CBus3PinSignal ParseCBus3FT232Signal(string text) {
        return text switch {
            "TXDEN" => Ftdi232RDevice.CBus3PinSignal.TxdEnable,
            "PWREN#" => Ftdi232RDevice.CBus3PinSignal.PowerEnable,
            "RXLED#" => Ftdi232RDevice.CBus3PinSignal.RxLed,
            "TXLED#" => Ftdi232RDevice.CBus3PinSignal.TxLed,
            "TX&RXLED#" => Ftdi232RDevice.CBus3PinSignal.TxRxLed,
            "SLEEP#" => Ftdi232RDevice.CBus3PinSignal.Sleep,
            "CLK48" => Ftdi232RDevice.CBus3PinSignal.Clock48Mhz,
            "CLK24" => Ftdi232RDevice.CBus3PinSignal.Clock24Mhz,
            "CLK12" => Ftdi232RDevice.CBus3PinSignal.Clock12Mhz,
            "CLK6" => Ftdi232RDevice.CBus3PinSignal.Clock6Mhz,
            "I/O MODE" => Ftdi232RDevice.CBus3PinSignal.IOMode,
            "BitBang WR#" => Ftdi232RDevice.CBus3PinSignal.BitBangWr,
            "BitBang RD#" => Ftdi232RDevice.CBus3PinSignal.BitBangRd,
            "WR#" => Ftdi232RDevice.CBus3PinSignal.Wr,
            _ => throw new InvalidOperationException($"Unknown CBus signal: {text}"),
        };
    }

    private static Ftdi232RDevice.CBus4PinSignal ParseCBus4FT232Signal(string text) {
        return text switch {
            "TXDEN" => Ftdi232RDevice.CBus4PinSignal.TxdEnable,
            "PWREN#" => Ftdi232RDevice.CBus4PinSignal.PowerEnable,
            "RXLED#" => Ftdi232RDevice.CBus4PinSignal.RxLed,
            "TXLED#" => Ftdi232RDevice.CBus4PinSignal.TxLed,
            "TX&RXLED#" => Ftdi232RDevice.CBus4PinSignal.TxRxLed,
            "SLEEP#" => Ftdi232RDevice.CBus4PinSignal.Sleep,
            "CLK48" => Ftdi232RDevice.CBus4PinSignal.Clock48Mhz,
            "CLK24" => Ftdi232RDevice.CBus4PinSignal.Clock24Mhz,
            "CLK12" => Ftdi232RDevice.CBus4PinSignal.Clock12Mhz,
            "CLK6" => Ftdi232RDevice.CBus4PinSignal.Clock6Mhz,
            _ => throw new InvalidOperationException($"Unknown CBus signal: {text}"),
        };
    }

    private static FtdiXSeriesDevice.CBusPinSignal ParseCBus4FTXSeriesSignal(string text) {
        return text switch {
            "Tristate" => FtdiXSeriesDevice.CBusPinSignal.Tristate,
            "RXLED" => FtdiXSeriesDevice.CBusPinSignal.RxLed,
            "TXLED" => FtdiXSeriesDevice.CBusPinSignal.TxLed,
            "TX&RXLED#" => FtdiXSeriesDevice.CBusPinSignal.TxRxLed,
            "PWREN#" => FtdiXSeriesDevice.CBusPinSignal.PwrEn,
            "SLEEP#" => FtdiXSeriesDevice.CBusPinSignal.Sleep,
            "Drive_0" => FtdiXSeriesDevice.CBusPinSignal.Drive0,
            "Drive_1" => FtdiXSeriesDevice.CBusPinSignal.Drive1,
            "GPIO" => FtdiXSeriesDevice.CBusPinSignal.Gpio,
            "TXDEN" => FtdiXSeriesDevice.CBusPinSignal.TxdEn,
            "CLK24" => FtdiXSeriesDevice.CBusPinSignal.Clock24Mhz,
            "CLK12" => FtdiXSeriesDevice.CBusPinSignal.Clock12Mhz,
            "CLK6" => FtdiXSeriesDevice.CBusPinSignal.Clock6Mhz,
            "BCD_Charger" => FtdiXSeriesDevice.CBusPinSignal.BcdCharger,
            "BCD_Charger#" => FtdiXSeriesDevice.CBusPinSignal.BcdChargerN,
            "I2C_TXE#" => FtdiXSeriesDevice.CBusPinSignal.I2CTxE,
            "I2C_RXF#" => FtdiXSeriesDevice.CBusPinSignal.I2CRxF,
            "VBUS_Sense" => FtdiXSeriesDevice.CBusPinSignal.VbusSense,
            "BitBang WR#" => FtdiXSeriesDevice.CBusPinSignal.BitBangWr,
            "BitBang RD#" => FtdiXSeriesDevice.CBusPinSignal.BitBangRd,
            "Time_Stamp" => FtdiXSeriesDevice.CBusPinSignal.TimeStamp,
            "Keep_Awake#" => FtdiXSeriesDevice.CBusPinSignal.KeepAwake,
            _ => throw new InvalidOperationException($"Unknown CBus signal: {text}"),
        };
    }

}
