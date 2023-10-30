namespace AltFTProg;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;

internal static class App {
    internal static void Main(string[] args) {
        var fileArgument = new Argument<FileInfo?>(
            name: "file",
            description: "Template file to use"
        ) {
            Arity = ArgumentArity.ZeroOrOne,
        };
        fileArgument.AddValidator(commandResult => {
            if (commandResult.GetValueOrDefault() is FileInfo source) {
                if (!source.Exists) {
                    commandResult.ErrorMessage = $"File \"{source.FullName}\" doesn't exist";
                }
            }
        });

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Increase verbosity level") {
            Arity = ArgumentArity.Zero,
            IsRequired = false,
        };

        var rootCommand = new RootCommand() {
            fileArgument,
            verboseOption,
        };
        rootCommand.SetHandler(
            (file, isVerbose) => {
                Run(file, isVerbose);
            },
            fileArgument, verboseOption);
        rootCommand.Invoke(args);
    }

    private static void Run(FileInfo? file, bool isVerbose) {
        XmlSimplified? xml = null;
        if (file != null) {
            xml = new XmlSimplified(file);
            if (isVerbose) {
                Console.WriteLine("Template " + file.FullName);
                if (isVerbose) {
                    Console.WriteLine("  Chip type: " + xml.ChipType);
                    Console.WriteLine("  Properties:");
                    foreach (var property in xml.Properties) {
                        Console.WriteLine("    " + property.Key + ": " + property.Value);
                    }
                }
            }
        }

        var devices = FtdiDevice.GetDevices();
        if (devices.Count == 0) {
            Console.Error.WriteLine("No devices found!");
            Environment.Exit(1);
        }

        foreach (var device in devices) {
            string deviceTitle;
            if (string.IsNullOrEmpty(device.UsbSerialNumber)) {
                deviceTitle = "FTDI " + GetDeviceTypeShortText(device) + " (no serial number)";
            } else {
                deviceTitle = "FTDI " + GetDeviceTypeShortText(device) + " (" + device.UsbSerialNumber + ")";
            }

            if ((xml != null) && xml.IsMatchingDevice(device)) { deviceTitle += " ⮜"; }
            Console.WriteLine(deviceTitle);
            if (isVerbose) {
                WriteDeviceDetails(device, includeEepromExtras: true);
            } else {
                Console.WriteLine("  USB Vendor ID .,,....: 0x" + device.UsbVendorId.ToString("X4"));
                Console.WriteLine("  USB Product ID ......: 0x" + device.UsbProductId.ToString("X4"));
                Console.WriteLine("  USB Manufacturer ....: " + device.UsbManufacturer);
                Console.WriteLine("  USB Product .........: " + device.UsbProductDescription);
                Console.WriteLine("  USB Serial ..........: " + device.UsbSerialNumber);

            }
        }
    }

    private static void WriteDeviceDetails(FtdiDevice device, bool includeEepromExtras) {
        Console.WriteLine("  Device type .........: " + GetDeviceTypeText(device));
        Console.WriteLine("  EEPROM size .........: " + device.EepromSize.ToString());

        Console.WriteLine("  Vendor ID ...........: 0x" + device.VendorId.ToString("X4"));
        Console.WriteLine("  Product ID ..........: 0x" + device.ProductId.ToString("X4"));
        Console.WriteLine("  Power source ........: " + GetBooleanText(device.SelfPowered, "Self-powered", "Bus-powered"));
        Console.WriteLine("  Maximum bus power ...: " + device.MaxBusPower.ToString() + " mA");
        Console.WriteLine("  Remote wakeup .......: " + GetBooleanText(device.RemoteWakeup, "Enabled", "Disabled"));
        Console.WriteLine("  IO during suspend ...: " + GetBooleanText(device.PulldownPinsInSuspend, "Pulled-down", "Floating"));
        Console.WriteLine("  Manufacturer ........: " + device.Manufacturer);
        Console.WriteLine("  Product .............: " + device.ProductDescription);
        Console.WriteLine("  Serial number enabled: " + GetBooleanText(device.SerialNumberEnabled, "Yes", "No"));
        Console.WriteLine("  Serial ..............: " + device.SerialNumber);
        if (device is Ftdi232RDevice device232R) {
            Console.WriteLine("  TXD inverted ........: " + GetBooleanText(device232R.TxdInverted, "Yes", "No"));
            Console.WriteLine("  RXD inverted ........: " + GetBooleanText(device232R.RxdInverted, "Yes", "No"));
            Console.WriteLine("  RTS inverted ........: " + GetBooleanText(device232R.RtsInverted, "Yes", "No"));
            Console.WriteLine("  CTS inverted ........: " + GetBooleanText(device232R.CtsInverted, "Yes", "No"));
            Console.WriteLine("  DTR inverted ........: " + GetBooleanText(device232R.DtrInverted, "Yes", "No"));
            Console.WriteLine("  DSR inverted ........: " + GetBooleanText(device232R.DsrInverted, "Yes", "No"));
            Console.WriteLine("  DCD inverted ........: " + GetBooleanText(device232R.DcdInverted, "Yes", "No"));
            Console.WriteLine("  RI inverted .........: " + GetBooleanText(device232R.RiInverted, "Yes", "No"));
            Console.WriteLine("  CBUS0 signal ........: " + GetPinText(device232R.CBus0Signal));
            Console.WriteLine("  CBUS1 signal ........: " + GetPinText(device232R.CBus1Signal));
            Console.WriteLine("  CBUS2 signal ........: " + GetPinText(device232R.CBus2Signal));
            Console.WriteLine("  CBUS3 signal ........: " + GetPinText(device232R.CBus3Signal));
            Console.WriteLine("  CBUS4 signal ........: " + GetPinText(device232R.CBus4Signal));
            Console.WriteLine("  High-current IO .....: " + GetBooleanText(device232R.IsHighCurrentIO, "Yes", "No"));
        } else if (device is FtdiXSeriesDevice deviceXSeries) {
            Console.WriteLine("  RS485 echo supression: " + GetBooleanText(deviceXSeries.Rs485EchoSuppression, "Enabled", "Disabled"));
            Console.WriteLine("  Driver ..............: " + GetBooleanText(deviceXSeries.D2xxDirectDriver, "D2XX Direct", "Virtual COM Port"));
            Console.WriteLine("  Battery charge ......: " + GetBooleanText(deviceXSeries.BatteryChargeEnable, "Enabled", "Disabled"));
            Console.WriteLine("  Force power enable ..: " + GetBooleanText(deviceXSeries.ForcePowerEnable, "Forced", "Normal"));
            Console.WriteLine("  Deactivate sleep ....: " + GetBooleanText(deviceXSeries.DeactivateSleep, "Yes", "No"));
            Console.WriteLine("  TXD inverted ........: " + GetBooleanText(deviceXSeries.TxdInverted, "Yes", "No"));
            Console.WriteLine("  RXD inverted ........: " + GetBooleanText(deviceXSeries.RxdInverted, "Yes", "No"));
            Console.WriteLine("  RTS inverted ........: " + GetBooleanText(deviceXSeries.RtsInverted, "Yes", "No"));
            Console.WriteLine("  CTS inverted ........: " + GetBooleanText(deviceXSeries.CtsInverted, "Yes", "No"));
            Console.WriteLine("  DTR inverted ........: " + GetBooleanText(deviceXSeries.DtrInverted, "Yes", "No"));
            Console.WriteLine("  DSR inverted ........: " + GetBooleanText(deviceXSeries.DsrInverted, "Yes", "No"));
            Console.WriteLine("  DCD inverted ........: " + GetBooleanText(deviceXSeries.DcdInverted, "Yes", "No"));
            Console.WriteLine("  RI inverted .........: " + GetBooleanText(deviceXSeries.RiInverted, "Yes", "No"));
            Console.WriteLine("  CBUS0 signal ........: " + GetPinText(deviceXSeries.CBus0Signal));
            Console.WriteLine("  CBUS1 signal ........: " + GetPinText(deviceXSeries.CBus1Signal));
            Console.WriteLine("  CBUS2 signal ........: " + GetPinText(deviceXSeries.CBus2Signal));
            Console.WriteLine("  CBUS3 signal ........: " + GetPinText(deviceXSeries.CBus3Signal));
            Console.WriteLine("  DBUS slow slew ......: " + GetBooleanText(deviceXSeries.DBusSlowSlew, "Yes", "No"));
            Console.WriteLine("  DBUS drive current ..: " + deviceXSeries.DBusDriveCurrent.ToString() + " mA");
            Console.WriteLine("  DBUS schmitt input ..: " + GetBooleanText(deviceXSeries.DBusSchmittInput, "Yes", "No"));
            Console.WriteLine("  CBUS slow slew ......: " + GetBooleanText(deviceXSeries.CBusSlowSlew, "Yes", "No"));
            Console.WriteLine("  CBUS drive current ..: " + deviceXSeries.CBusDriveCurrent.ToString() + " mA");
            Console.WriteLine("  CBUS schmitt input ..: " + GetBooleanText(deviceXSeries.CBusSchmittInput, "Yes", "No"));
        }
        Console.WriteLine("  Checksum ............: " + GetBooleanText(device.IsChecksumValid, "Valid", "Invalid"));

        Console.WriteLine("  EEPROM");
        var eepromBytes = device.GetEepromBytes(includeEepromExtras);
        for (var i = 0; i < eepromBytes.Length; i += 16) {
            var sbHex = new StringBuilder();
            var sbAscii = new StringBuilder();

            sbHex.Append(i.ToString("X2") + ": ");
            for (var j = 0; j < 16; j++) {
                if (j == 8) {
                    sbHex.Append(' ');
                    sbAscii.Append(' ');
                }

                var b = eepromBytes[i + j];

                sbHex.Append(b.ToString("X2") + " ");
                sbAscii.Append((b is < 32 or > 126) ? '·' : (char)b);
            }
            Console.WriteLine("    " + sbHex.ToString() + " " + sbAscii.ToString());
        }
    }

    private static string GetDeviceTypeText(FtdiDevice device) {
        string classType;
        if (device is Ftdi232RDevice) {
            classType = "232R";
        } else if (device is FtdiXSeriesDevice) {
            classType = "FT X Series";
        } else {
            classType = "Unknown";
        }
        var type = device.DeviceType;
        return type switch {
            FtdiDeviceType.FT232A => "FT232/245AM (" + ((int)type).ToString() + ")",
            FtdiDeviceType.FT232B => "FT232/245BM (" + ((int)type).ToString() + ")",
            FtdiDeviceType.FT2232D => "FT2232D (" + ((int)type).ToString() + ")",
            FtdiDeviceType.FT232R => "FT232R/FT245R (" + ((int)type).ToString() + ")",
            FtdiDeviceType.FT2232H => "FT2232H (" + ((int)type).ToString() + ")",
            FtdiDeviceType.FT232H => "FT232H (" + ((int)type).ToString() + ")",
            FtdiDeviceType.FTXSeries => "FT X Series (" + ((int)type).ToString() + ")",
            _ => "(" + ((int)type).ToString() + ") " + classType,
        };
    }

    private static string GetDeviceTypeShortText(FtdiDevice device) {
        if (device is Ftdi232RDevice) {
            return "232R";
        } else if (device is FtdiXSeriesDevice) {
            return "X Series";
        } else {
            var type = device.DeviceType;
            return type switch {
                FtdiDeviceType.FT232A => "232/245AM",
                FtdiDeviceType.FT232B => "232/245BM",
                FtdiDeviceType.FT2232D => "2232D",
                FtdiDeviceType.FT232R => "232R/245R",
                FtdiDeviceType.FT2232H => "2232H",
                FtdiDeviceType.FT232H => "232H",
                FtdiDeviceType.FTXSeries => "X Series",
                _ => "(unknown)",
            };
        }
    }

    private static string GetPinText(Ftdi232RDevice.CBus0PinSignal? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus0PinSignal.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinSignal.RxF => "RXF# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus1PinSignal? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus1PinSignal.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinSignal.TxE => "TXE# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus2PinSignal? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus2PinSignal.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinSignal.Rd => "RD# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus3PinSignal? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus3PinSignal.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinSignal.Wr => "WR# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus4PinSignal? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus4PinSignal.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinSignal.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(FtdiXSeriesDevice.CBusPinSignal? function) {
        return function switch {
            null => "",
            FtdiXSeriesDevice.CBusPinSignal.Tristate => "Tristate (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.PwrEn => "PWREN# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.Drive0 => "Drive_0 (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.Drive1 => "Drive_1 (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.Gpio => "GPIO (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.TxdEn => "TXDEN (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.BcdCharger => "BCD_Charger (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.BcdChargerN => "BCD_Charger# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.I2CTxE => "I2C_TXE# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.I2CRxF => "I2C_RXF# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.VbusSense => "VBUS_Sense (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.BitBangWr => "BitBang_WR# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.BitBangRd => "BitBang_RD# (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.TimeStamp => "Time_Stamp (" + ((int)function).ToString() + ")",
            FtdiXSeriesDevice.CBusPinSignal.KeepAwake => "Keep_Awake (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetBooleanText(bool? value, string textIfTrue, string textIfFalse) {
        if (value == null) { return ""; }
        return value.Value ? textIfTrue : textIfFalse;
    }
}
