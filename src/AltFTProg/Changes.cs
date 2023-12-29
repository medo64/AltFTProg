namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

internal static class Changes {

    public static bool Apply(FtdiDevice device, ReadOnlyCollection<KeyValuePair<string, string>> properties) {
        var deviceCommon = device as FtdiCommonDevice;
        var device232R = device as Ftdi232RDevice;
        var deviceXSeries = device as FtdiXSeriesDevice;

        var newManufacturer = deviceCommon?.Manufacturer ?? "";
        var newProductDescription = deviceCommon?.ProductDescription ?? "";
        var newSerialNumber = deviceCommon?.SerialNumber ?? "";
        var newSerialNumberPrefix = "";
        var newSerialNumberGenerate = false;
        var newSerialNumberEnabled = deviceCommon?.SerialNumberEnabled ?? (newSerialNumber.Length > 0);

        foreach (var property in properties) {
            var name = property.Key;
            var value = property.Value;
            switch (name) {
                case "USB_Device_Descriptor/VID_PID": {
                        if (!value.Equals("0")) { Output.WriteWarningLine($"  Cannot set {name}: {value}"); break; }
                    }
                    break;

                case "USB_Device_Descriptor/idVendor": {
                        var newVendorId = ushort.Parse(value, NumberStyles.HexNumber);
                        if (deviceCommon != null) {
                            deviceCommon.VendorId = newVendorId;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "USB_Device_Descriptor/idProduct": {
                        var newProductId = ushort.Parse(value, NumberStyles.HexNumber);
                        if (deviceCommon != null) {
                            deviceCommon.ProductId = newProductId;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "USB_Device_Descriptor/bcdUSB":
                    // ignore it
                    break;

                case "bmAttributes/RemoteWakeupEnabled": {
                        var newRemoteWakeupEnabled = bool.Parse(value);
                        if (deviceCommon != null) {
                            deviceCommon.RemoteWakeupEnabled = newRemoteWakeupEnabled;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "bmAttributes/SelfPowered": {
                        var newSelfPowered = bool.Parse(value);
                        if (deviceCommon != null) {
                            deviceCommon.SelfPowered = newSelfPowered;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "bmAttributes/BusPowered": {
                        var newBusPowered = bool.Parse(value);
                        if (deviceCommon != null) {
                            deviceCommon.BusPowered = newBusPowered;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "USB_Config_Descriptor/IOpullDown":
                    var newPulldownPinsInSuspend = bool.Parse(value);
                    if (deviceCommon != null) {
                        deviceCommon.PulldownPinsInSuspend = newPulldownPinsInSuspend;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "USB_Config_Descriptor/MaxPower": {
                        var newMaxBusPower = int.Parse(value);
                        if (deviceCommon != null) {
                            deviceCommon.MaxBusPower = newMaxBusPower;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
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

                case "USB_String_Descriptors/SerialNumberPrefix":
                    newSerialNumberPrefix = value.Trim();
                    break;

                case "USB_String_Descriptors/SerialNumber_AutoGenerate": {
                        if (bool.Parse(value)) {
                            newSerialNumberGenerate = true;
                        }
                    }
                    break;

                case "Hardware_Specific/HighIO": {
                        var newHighCurrentIO = bool.Parse(value);
                        if (device232R != null) {
                            device232R.HighCurrentIO = newHighCurrentIO;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Hardware_Specific/D2XX":
                case "Driver/D2XX": {
                        var newD2xxDirectDriver = bool.Parse(value);
                        if (device232R != null) {
                            device232R.D2xxDirectDriver = newD2xxDirectDriver;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.D2xxDirectDriver = newD2xxDirectDriver;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Driver/VCP": {
                        var newVirtualComPortDriver = bool.Parse(value);
                        if (deviceXSeries != null) {
                            deviceXSeries.VirtualComPortDriver = newVirtualComPortDriver;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;


                case "Hardware_Specific/ExternalOscillator": {
                        var newExternalOscillator = bool.Parse(value);
                        if (newExternalOscillator) {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Hardware_Specific/USB_Suspend_VBus":
                    //ignore
                    break;

                case "Hardware_Specific/RS485_Echo_Suppress": {
                        var newRs485EchoSuppression = bool.Parse(value);
                        if (deviceXSeries != null) {
                            deviceXSeries.Rs485EchoSuppression = newRs485EchoSuppression;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Hardware/Device":
                    //ignore
                    break;

                case "Invert_RS232_Signals/TXD": {
                        var newTxdInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.TxdInverted = newTxdInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.TxdInverted = newTxdInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Invert_RS232_Signals/RXD": {
                        var newRxdInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.RxdInverted = newRxdInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.RxdInverted = newRxdInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Invert_RS232_Signals/RTS": {
                        var newRtsInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.RtsInverted = newRtsInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.RtsInverted = newRtsInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Invert_RS232_Signals/CTS": {
                        var newCtsInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.CtsInverted = newCtsInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.CtsInverted = newCtsInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Invert_RS232_Signals/DTR": {
                        var newDtrInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.DtrInverted = newDtrInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.DtrInverted = newDtrInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Invert_RS232_Signals/DSR": {
                        var newDsrInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.DsrInverted = newDsrInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.DsrInverted = newDsrInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Invert_RS232_Signals/DCD": {
                        var newDcdInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.DcdInverted = newDcdInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.DcdInverted = newDcdInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Invert_RS232_Signals/RI": {
                        var newRiInverted = bool.Parse(value);
                        if (device232R != null) {
                            device232R.RiInverted = newRiInverted;
                        } else if (deviceXSeries != null) {
                            deviceXSeries.RiInverted = newRiInverted;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "IO_Controls/C0": {
                        var newCBus0Signal = ParseCBus0FT232Signal(value);
                        if (device232R != null) {
                            device232R.CBus0Signal = newCBus0Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "IO_Controls/C1": {
                        var newCBus1Signal = ParseCBus1FT232Signal(value);
                        if (device232R != null) {
                            device232R.CBus1Signal = newCBus1Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "IO_Controls/C2": {
                        var newCBus2Signal = ParseCBus2FT232Signal(value);
                        if (device232R != null) {
                            device232R.CBus2Signal = newCBus2Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "IO_Controls/C3": {
                        var newCBus3Signal = ParseCBus3FT232Signal(value);
                        if (device232R != null) {
                            device232R.CBus3Signal = newCBus3Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "IO_Controls/C4": {
                        var newCBus4Signal = ParseCBus4FT232Signal(value);
                        if (device232R != null) {
                            device232R.CBus4Signal = newCBus4Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "CBUS_Signals/C0": {
                        var newCBus0Signal = ParseCBus4FTXSeriesSignal(value);
                        if (deviceXSeries != null) {
                            deviceXSeries.CBus0Signal = newCBus0Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "CBUS_Signals/C1": {
                        var newCBus1Signal = ParseCBus4FTXSeriesSignal(value);
                        if (deviceXSeries != null) {
                            deviceXSeries.CBus1Signal = newCBus1Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "CBUS_Signals/C2": {
                        var newCBus2Signal = ParseCBus4FTXSeriesSignal(value);
                        if (deviceXSeries != null) {
                            deviceXSeries.CBus2Signal = newCBus2Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "CBUS_Signals/C3": {
                        var newCBus3Signal = ParseCBus4FTXSeriesSignal(value);
                        if (deviceXSeries != null) {
                            deviceXSeries.CBus3Signal = newCBus3Signal;
                        } else {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    }
                    break;

                case "Battery_Charge_Detect/Enable":
                    var newBatteryChargeEnable = bool.Parse(value);
                    if (deviceXSeries != null) {
                        deviceXSeries.BatteryChargeEnable = newBatteryChargeEnable;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Battery_Charge_Detect/Power_Enable":
                    var newForcePowerEnable = bool.Parse(value);
                    if (deviceXSeries != null) {
                        deviceXSeries.ForcePowerEnable = newForcePowerEnable;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Battery_Charge_Detect/Deactivate_Sleep":
                    var newDeactivateSleep = bool.Parse(value);
                    if (deviceXSeries != null) {
                        deviceXSeries.DeactivateSleep = newDeactivateSleep;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "DBUS/SlowSlew":
                    var newDBusSlowSlew = bool.Parse(value);
                    if (deviceXSeries != null) {
                        deviceXSeries.DBusSlowSlew = newDBusSlowSlew;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "DBUS/Drive":
                    var newDBusDriveCurrent = int.Parse(value.Replace("mA", ""), NumberStyles.Integer, CultureInfo.InvariantCulture);
                    if (deviceXSeries != null) {
                        deviceXSeries.DBusDriveCurrent = newDBusDriveCurrent;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "DBUS/Schmitt":
                    var newDBusSchmittInput = bool.Parse(value);
                    if (deviceXSeries != null) {
                        deviceXSeries.DBusSchmittInput = newDBusSchmittInput;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "CBUS/SlowSlew":
                    var newCBusSlowSlew = bool.Parse(value);
                    if (deviceXSeries != null) {
                        deviceXSeries.CBusSlowSlew = newCBusSlowSlew;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "CBUS/Drive":
                    var newCBusDriveCurrent = int.Parse(value.Replace("mA", ""), NumberStyles.Integer, CultureInfo.InvariantCulture);
                    if (deviceXSeries != null) {
                        deviceXSeries.CBusDriveCurrent = newCBusDriveCurrent;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "CBUS/Schmitt":
                    var newCBusSchmittInput = bool.Parse(value);
                    if (deviceXSeries != null) {
                        deviceXSeries.CBusSchmittInput = newCBusSchmittInput;
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                default: Output.WriteWarningLine($"  Unknown property {name}: {value}"); break;
            }
        }

        if (newSerialNumberGenerate) { newSerialNumber = FtdiDevice.GetRandomSerialNumber(newSerialNumberPrefix, 6); }

        if (deviceCommon != null) {
            deviceCommon.SetStringDescriptors(newManufacturer, newProductDescription, newSerialNumber);
            deviceCommon.SerialNumberEnabled = newSerialNumberEnabled && (deviceCommon.SerialNumber.Length > 0);
        } else {
            Output.WriteWarningLine($"  Cannot set USB string descriptors");
        }

        return device.HasEepromChanged;
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