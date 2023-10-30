namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

internal static class Changes {

    public static bool Apply(FtdiDevice device, ReadOnlyCollection<KeyValuePair<string, string>> properties) {
        var device232R = device as Ftdi232RDevice;
        var deviceXSeries = device as FtdiXSeriesDevice;

        var hasModified = false;
        string serialNumberPrefix = "";

        foreach (var property in properties) {
            var name = property.Key;
            var value = property.Value;
            switch (name) {
                case "USB_Device_Descriptor/VID_PID":
                    if (!value.Equals("0")) { Output.WriteWarningLine($"  Cannot set {name}: {value}"); break; }
                    break;

                case "USB_Device_Descriptor/idVendor":
                    var newVendorId = ushort.Parse(value, NumberStyles.HexNumber);
                    if (device.VendorId != newVendorId) {
                        device.VendorId = newVendorId;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_Device_Descriptor/idProduct":
                    var newProductId = ushort.Parse(value, NumberStyles.HexNumber);
                    if (device.ProductId != newProductId) {
                        device.ProductId = newProductId;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_Device_Descriptor/bcdUSB":
                    // ignore it
                    break;

                case "bmAttributes/RemoteWakeupEnabled":
                    var newRemoteWakeupEnabled = bool.Parse(value);
                    if (device.RemoteWakeupEnabled != newRemoteWakeupEnabled) {
                        device.RemoteWakeupEnabled = newRemoteWakeupEnabled;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "bmAttributes/SelfPowered":
                    var newSelfPowered = bool.Parse(value);
                    if (device.SelfPowered != newSelfPowered) {
                        device.SelfPowered = newSelfPowered;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "bmAttributes/BusPowered":
                    var newBusPowered = bool.Parse(value);
                    if (device.BusPowered != newBusPowered) {
                        device.BusPowered = newBusPowered;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_Config_Descriptor/IOpullDown":
                    var newPulldownPinsInSuspend = bool.Parse(value);
                    if (device.PulldownPinsInSuspend != newPulldownPinsInSuspend) {
                        device.PulldownPinsInSuspend = newPulldownPinsInSuspend;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_Config_Descriptor/MaxPower":
                    var newMaxBusPower = int.Parse(value);
                    if (device.MaxBusPower != newMaxBusPower) {
                        device.MaxBusPower = newMaxBusPower;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_String_Descriptors/Manufacturer":
                    var newManufacturer = value.Trim();
                    if (!device.Manufacturer.Equals(newManufacturer, StringComparison.Ordinal)) {
                        device.Manufacturer = newManufacturer;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_String_Descriptors/Product_Description":
                    var newProductDescription = value.Trim();
                    if (!device.ProductDescription.Equals(newProductDescription, StringComparison.Ordinal)) {
                        device.ProductDescription = newProductDescription;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_String_Descriptors/SerialNumber_Enabled":
                    var newSerialNumberEnabled = bool.Parse(value);
                    if (device.SerialNumberEnabled != newSerialNumberEnabled) {
                        device.SerialNumberEnabled = newSerialNumberEnabled;
                        Output.WriteLine($"  Setting {name}: {value}");
                        hasModified = true;
                    }
                    break;

                case "USB_String_Descriptors/SerialNumberPrefix":
                    serialNumberPrefix = value.Trim();
                    break;

                case "USB_String_Descriptors/SerialNumber_AutoGenerate":
                    if (bool.Parse(value)) {
                        var newSerialNumber = Helpers.GetRandomSerial(serialNumberPrefix, 6);
                        device.SerialNumber = newSerialNumber;
                        Output.WriteLine($"  Setting SerialNumber: {newSerialNumber}");
                        hasModified = true;
                    }
                    break;

                case "Hardware_Specific/HighIO":
                    var newHighCurrentIO = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.HighCurrentIO != newHighCurrentIO) {
                            device232R.HighCurrentIO = newHighCurrentIO;
                            Output.WriteLine($"  Setting {name}: {value}");
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Hardware_Specific/D2XX":
                    var newD2xxDirectDriver = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.D2xxDirectDriver != newD2xxDirectDriver) {
                            device232R.D2xxDirectDriver = newD2xxDirectDriver;
                            Output.WriteLine($"  Setting {name}: {value}");
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.D2xxDirectDriver != newD2xxDirectDriver) {
                            deviceXSeries.D2xxDirectDriver = newD2xxDirectDriver;
                            Output.WriteLine($"  Setting {name}: {value}");
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Hardware_Specific/ExternalOscillator":
                    var newExternalOscillator = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.ExternalOscillator != newExternalOscillator) {
                            Output.WriteWarningLine($"  Cannot set {name}: {value}");
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/TXD":
                    var newTxdInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.TxdInverted != newTxdInverted) {
                            device232R.TxdInverted = newTxdInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.TxdInverted != newTxdInverted) {
                            deviceXSeries.TxdInverted = newTxdInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/RXD":
                    var newRxdInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.RxdInverted != newRxdInverted) {
                            device232R.RxdInverted = newRxdInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.RxdInverted != newRxdInverted) {
                            deviceXSeries.RxdInverted = newRxdInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/RTS":
                    var newRtsInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.RtsInverted != newRtsInverted) {
                            device232R.RtsInverted = newRtsInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.RtsInverted != newRtsInverted) {
                            deviceXSeries.RtsInverted = newRtsInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/CTS":
                    var newCtsInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.CtsInverted != newCtsInverted) {
                            device232R.CtsInverted = newCtsInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.CtsInverted != newCtsInverted) {
                            deviceXSeries.CtsInverted = newCtsInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/DTR":
                    var newDtrInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.DtrInverted != newDtrInverted) {
                            device232R.DtrInverted = newDtrInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.DtrInverted != newDtrInverted) {
                            deviceXSeries.DtrInverted = newDtrInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/DSR":
                    var newDsrInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.DsrInverted != newDsrInverted) {
                            device232R.DsrInverted = newDsrInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.DsrInverted != newDsrInverted) {
                            deviceXSeries.DsrInverted = newDsrInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/DCD":
                    var newDcdInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.DcdInverted != newDcdInverted) {
                            device232R.DcdInverted = newDcdInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.DcdInverted != newDcdInverted) {
                            deviceXSeries.DcdInverted = newDcdInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "Invert_RS232_Signals/RI":
                    var newRiInverted = bool.Parse(value);
                    if (device232R != null) {
                        if (device232R.RiInverted != newRiInverted) {
                            device232R.RiInverted = newRiInverted;
                            hasModified = true;
                        }
                    } else if (deviceXSeries != null) {
                        if (deviceXSeries.RiInverted != newRiInverted) {
                            deviceXSeries.RiInverted = newRiInverted;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "IO_Controls/C0":
                    var newCBus0Signal = ParseCBus0FT232Signal(value);
                    if (device232R != null) {
                        if (device232R.CBus0Signal != newCBus0Signal) {
                            device232R.CBus0Signal = newCBus0Signal;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "IO_Controls/C1":
                    var newCBus1Signal = ParseCBus1FT232Signal(value);
                    if (device232R != null) {
                        if (device232R.CBus1Signal != newCBus1Signal) {
                            device232R.CBus1Signal = newCBus1Signal;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "IO_Controls/C2":
                    var newCBus2Signal = ParseCBus2FT232Signal(value);
                    if (device232R != null) {
                        if (device232R.CBus2Signal != newCBus2Signal) {
                            device232R.CBus2Signal = newCBus2Signal;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "IO_Controls/C3":
                    var newCBus3Signal = ParseCBus3FT232Signal(value);
                    if (device232R != null) {
                        if (device232R.CBus3Signal != newCBus3Signal) {
                            device232R.CBus3Signal = newCBus3Signal;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                case "IO_Controls/C4":
                    var newCBus4Signal = ParseCBus4FT232Signal(value);
                    if (device232R != null) {
                        if (device232R.CBus4Signal != newCBus4Signal) {
                            device232R.CBus4Signal = newCBus4Signal;
                            hasModified = true;
                        }
                    } else {
                        Output.WriteWarningLine($"  Cannot set {name}: {value}");
                    }
                    break;

                default: Output.WriteWarningLine($"  Unknown property {name}: {value}"); break;
            }
        }

        return hasModified;
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
            _ => Ftdi232RDevice.CBus0PinSignal.TxdEnable,
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
            _ => Ftdi232RDevice.CBus1PinSignal.TxdEnable,
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
            _ => Ftdi232RDevice.CBus2PinSignal.TxdEnable,
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
            _ => Ftdi232RDevice.CBus3PinSignal.TxdEnable,
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
            _ => Ftdi232RDevice.CBus4PinSignal.TxdEnable,
        };
    }

    private static FtdiXSeriesDevice.CBusPinSignal ParseCBus4FTXSeriesSignal(string text) {
        return text switch {
            "Tristate" => FtdiXSeriesDevice.CBusPinSignal.Tristate,
            "RXLED#" => FtdiXSeriesDevice.CBusPinSignal.RxLed,
            "TXLED#" => FtdiXSeriesDevice.CBusPinSignal.TxLed,
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
            _ => FtdiXSeriesDevice.CBusPinSignal.TxdEn,
        };
    }

}