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

        var resetEepromOption = new Option<bool>(
            aliases: new[] { "--reset" },
            description: "Try to reset EEPROM to defaults") {
            Arity = ArgumentArity.Zero,
            IsRequired = false,
        };

        var fixChecksumOption = new Option<bool>(
            aliases: new[] { "--fix-checksum" },
            description: "Forces checksum fixup") {
            Arity = ArgumentArity.Zero,
            IsRequired = false,
        };

        var rootCommand = new RootCommand() {
            fileArgument,
            verboseOption,
            resetEepromOption,
            fixChecksumOption,
        };
        rootCommand.SetHandler(
            (file, isVerbose, resetEeprom, fixChecksum) => {
                Run(file, isVerbose, resetEeprom, fixChecksum);
            },
            fileArgument, verboseOption, resetEepromOption, fixChecksumOption);
        rootCommand.Invoke(args);
    }

    private static void Run(FileInfo? file, bool isVerbose, bool resetEeprom, bool fixChecksum) {
        FtdiXmlTemplate? template = null;
        if (file != null) {
            using var stream = file.OpenRead();
            template = FtdiXmlTemplate.Load(stream);
            if (isVerbose) {
                Output.WriteLine("Template " + file.FullName);
                if (isVerbose) {
                    Output.WriteVerboseLine("  Device type: " + template.DeviceType);
                    Output.WriteVerboseLine("  Properties:");
                    foreach (var property in template.Properties) {
                        Output.WriteVerboseLine("    " + property.Key + ": " + property.Value);
                    }
                }
            }
        }

        var devices = FtdiDevice.GetDevices();
        if (devices.Count == 0) {
            Output.WriteErrorLine("No devices found!");
            Environment.Exit(1);
        }

        foreach (var device in devices) {
            string deviceTitle;
            string usbSerialNumber = device.UsbSerialNumber;
            if (string.IsNullOrEmpty(usbSerialNumber) || "?".Equals(usbSerialNumber, StringComparison.Ordinal)) {
                deviceTitle = "FTDI " + GetDeviceTypeShortText(device) + " (no serial number)";
            } else {
                deviceTitle = "FTDI " + GetDeviceTypeShortText(device) + " (" + device.UsbSerialNumber + ")";
            }

            var shouldResetEeprom = resetEeprom && ((device is Ftdi232RDevice) || (device is FtdiXSeriesDevice));
            if (shouldResetEeprom) { deviceTitle += " ↺"; }

            if ((template != null) && (template.DeviceType == device.DeviceType)) { deviceTitle += " ⮜"; }
            Output.WriteLine(deviceTitle);

            Output.WriteLine("  USB Vendor ID .......: 0x" + device.UsbVendorId.ToString("X4"));
            Output.WriteLine("  USB Product ID ......: 0x" + device.UsbProductId.ToString("X4"));
            Output.WriteLine("  USB Manufacturer ....: " + device.UsbManufacturer);
            Output.WriteLine("  USB Product .........: " + device.UsbProductDescription);
            Output.WriteLine("  USB Serial ..........: " + device.UsbSerialNumber);

            if (shouldResetEeprom) {
                if (device is Ftdi232RDevice device232R) {
                    device232R.ResetEepromToDefaults();
                    device232R.SaveEeprom();
                } else if (device is FtdiXSeriesDevice deviceXSeries) {
                    deviceXSeries.ResetEepromToDefaults();
                    deviceXSeries.SaveEeprom();
                }
            }

            if (fixChecksum) {
                if (device is Ftdi232RDevice device232R) {
                    device232R.IsChecksumValid = true;
                    device232R.SaveEeprom();
                } else if (device is FtdiXSeriesDevice deviceXSeries) {
                    deviceXSeries.IsChecksumValid = true;
                    deviceXSeries.SaveEeprom();
                }
            }

            if (isVerbose) { WriteDeviceDetails(device); }

            if ((template != null) && (template.DeviceType == device.DeviceType)) {
                template.Apply(device);
                if (device.HasEepromChanged) {
                    device.SaveEeprom();
                    if (isVerbose) { WriteDeviceDetails(device); }
                }
            }
        }
    }

    private static void WriteDeviceDetails(FtdiDevice device) {
        Output.WriteVerboseLine("  Device type .........: " + GetDeviceTypeText(device));
        Output.WriteVerboseLine("  EEPROM size .........: " + device.EepromSize.ToString());

        if (device is Ftdi232RDevice device232R) {

            Output.WriteVerboseLine("  Oscillator ..........: " + GetBooleanText(device232R.ExternalOscillator, "External", "Internal"));
            Output.WriteVerboseLine("  High-current IO .....: " + GetBooleanText(device232R.HighCurrentIO, "Yes", "No"));
            Output.WriteVerboseLine("  Driver ..............: " + GetBooleanText(device232R.D2xxDirectDriver, "D2XX Direct", "Virtual COM Port"));

            Output.WriteVerboseLine("  Vendor ID ...........: 0x" + device232R.VendorId.ToString("X4"));
            Output.WriteVerboseLine("  Product ID ..........: 0x" + device232R.ProductId.ToString("X4"));

            Output.WriteVerboseLine("  Remote wakeup .......: " + GetBooleanText(device232R.RemoteWakeupEnabled, "Enabled", "Disabled"));
            Output.WriteVerboseLine("  Power source ........: " + GetBooleanText(device232R.SelfPowered, "Self-powered", "Bus-powered"));

            Output.WriteVerboseLine("  Maximum bus power ...: " + device232R.MaxBusPower.ToString() + " mA");

            Output.WriteVerboseLine("  IO during suspend ...: " + GetBooleanText(device232R.PulldownPinsInSuspend, "Pulled-down", "Floating"));
            Output.WriteVerboseLine("  Serial number enabled: " + GetBooleanText(device232R.SerialNumberEnabled, "Yes", "No"));

            Output.WriteVerboseLine("  TXD inverted ........: " + GetBooleanText(device232R.TxdInverted, "Yes", "No"));
            Output.WriteVerboseLine("  RXD inverted ........: " + GetBooleanText(device232R.RxdInverted, "Yes", "No"));
            Output.WriteVerboseLine("  RTS inverted ........: " + GetBooleanText(device232R.RtsInverted, "Yes", "No"));
            Output.WriteVerboseLine("  CTS inverted ........: " + GetBooleanText(device232R.CtsInverted, "Yes", "No"));
            Output.WriteVerboseLine("  DTR inverted ........: " + GetBooleanText(device232R.DtrInverted, "Yes", "No"));
            Output.WriteVerboseLine("  DSR inverted ........: " + GetBooleanText(device232R.DsrInverted, "Yes", "No"));
            Output.WriteVerboseLine("  DCD inverted ........: " + GetBooleanText(device232R.DcdInverted, "Yes", "No"));
            Output.WriteVerboseLine("  RI inverted .........: " + GetBooleanText(device232R.RiInverted, "Yes", "No"));

            Output.WriteVerboseLine("  Manufacturer ........: " + device232R.Manufacturer);
            Output.WriteVerboseLine("  Product .............: " + device232R.ProductDescription);
            Output.WriteVerboseLine("  Serial ..............: " + device232R.SerialNumber);

            Output.WriteVerboseLine("  CBUS0 signal ........: " + GetPinText(device232R.CBus0Signal));
            Output.WriteVerboseLine("  CBUS1 signal ........: " + GetPinText(device232R.CBus1Signal));
            Output.WriteVerboseLine("  CBUS2 signal ........: " + GetPinText(device232R.CBus2Signal));
            Output.WriteVerboseLine("  CBUS3 signal ........: " + GetPinText(device232R.CBus3Signal));
            Output.WriteVerboseLine("  CBUS4 signal ........: " + GetPinText(device232R.CBus4Signal));

            Output.WriteVerboseLine("  Checksum ............: " + GetBooleanText(device232R.IsChecksumValid, "Valid", "Invalid"));

        } else if (device is FtdiXSeriesDevice deviceXSeries) {

            Output.WriteVerboseLine("  Battery charge ......: " + GetBooleanText(deviceXSeries.BatteryChargeEnable, "Enabled", "Disabled"));
            Output.WriteVerboseLine("  Force power enable ..: " + GetBooleanText(deviceXSeries.ForcePowerEnable, "Forced", "Normal"));
            Output.WriteVerboseLine("  Deactivate sleep ....: " + GetBooleanText(deviceXSeries.DeactivateSleep, "Yes", "No"));
            Output.WriteVerboseLine("  RS485 echo supression: " + GetBooleanText(deviceXSeries.Rs485EchoSuppression, "Enabled", "Disabled"));
            Output.WriteVerboseLine("  Oscillator ..........: " + GetBooleanText(deviceXSeries.ExternalOscillator, "External", "Internal"));
            Output.WriteVerboseLine("  Oscillator resistor .: " + GetBooleanText(deviceXSeries.ExternalOscillatorFeedbackResistor, "Feedback", "No"));
            Output.WriteVerboseLine("  CBUS pin Vbus sense .: " + GetBooleanText(deviceXSeries.CbusPinVbusSense, "Yes", "No"));
            Output.WriteVerboseLine("  Driver ..............: " + GetBooleanText(deviceXSeries.D2xxDirectDriver, "D2XX Direct", "Virtual COM Port"));

            Output.WriteVerboseLine("  Vendor ID ...........: 0x" + deviceXSeries.VendorId.ToString("X4"));
            Output.WriteVerboseLine("  Product ID ..........: 0x" + deviceXSeries.ProductId.ToString("X4"));

            Output.WriteVerboseLine("  Remote wakeup .......: " + GetBooleanText(deviceXSeries.RemoteWakeupEnabled, "Enabled", "Disabled"));
            Output.WriteVerboseLine("  Power source ........: " + GetBooleanText(deviceXSeries.SelfPowered, "Self-powered", "Bus-powered"));

            Output.WriteVerboseLine("  Maximum bus power ...: " + deviceXSeries.MaxBusPower.ToString() + " mA");

            Output.WriteVerboseLine("  IO during suspend ...: " + GetBooleanText(deviceXSeries.PulldownPinsInSuspend, "Pulled-down", "Floating"));
            Output.WriteVerboseLine("  Serial number enabled: " + GetBooleanText(deviceXSeries.SerialNumberEnabled, "Yes", "No"));

            Output.WriteVerboseLine("  TXD inverted ........: " + GetBooleanText(deviceXSeries.TxdInverted, "Yes", "No"));
            Output.WriteVerboseLine("  RXD inverted ........: " + GetBooleanText(deviceXSeries.RxdInverted, "Yes", "No"));
            Output.WriteVerboseLine("  RTS inverted ........: " + GetBooleanText(deviceXSeries.RtsInverted, "Yes", "No"));
            Output.WriteVerboseLine("  CTS inverted ........: " + GetBooleanText(deviceXSeries.CtsInverted, "Yes", "No"));
            Output.WriteVerboseLine("  DTR inverted ........: " + GetBooleanText(deviceXSeries.DtrInverted, "Yes", "No"));
            Output.WriteVerboseLine("  DSR inverted ........: " + GetBooleanText(deviceXSeries.DsrInverted, "Yes", "No"));
            Output.WriteVerboseLine("  DCD inverted ........: " + GetBooleanText(deviceXSeries.DcdInverted, "Yes", "No"));
            Output.WriteVerboseLine("  RI inverted .........: " + GetBooleanText(deviceXSeries.RiInverted, "Yes", "No"));

            Output.WriteVerboseLine("  DBUS slow slew ......: " + GetBooleanText(deviceXSeries.DBusSlowSlew, "Yes", "No"));
            Output.WriteVerboseLine("  DBUS drive current ..: " + deviceXSeries.DBusDriveCurrent.ToString() + " mA");
            Output.WriteVerboseLine("  DBUS schmitt input ..: " + GetBooleanText(deviceXSeries.DBusSchmittInput, "Yes", "No"));
            Output.WriteVerboseLine("  CBUS slow slew ......: " + GetBooleanText(deviceXSeries.CBusSlowSlew, "Yes", "No"));
            Output.WriteVerboseLine("  CBUS drive current ..: " + deviceXSeries.CBusDriveCurrent.ToString() + " mA");
            Output.WriteVerboseLine("  CBUS schmitt input ..: " + GetBooleanText(deviceXSeries.CBusSchmittInput, "Yes", "No"));

            Output.WriteVerboseLine("  Manufacturer ........: " + deviceXSeries.Manufacturer);
            Output.WriteVerboseLine("  Product description .: " + deviceXSeries.ProductDescription);
            Output.WriteVerboseLine("  Serial number .......: " + deviceXSeries.SerialNumber);

            Output.WriteVerboseLine("  CBUS0 signal ........: " + GetPinText(deviceXSeries.CBus0Signal));
            Output.WriteVerboseLine("  CBUS1 signal ........: " + GetPinText(deviceXSeries.CBus1Signal));
            Output.WriteVerboseLine("  CBUS2 signal ........: " + GetPinText(deviceXSeries.CBus2Signal));
            Output.WriteVerboseLine("  CBUS3 signal ........: " + GetPinText(deviceXSeries.CBus3Signal));

            Output.WriteVerboseLine("  Checksum ............: " + GetBooleanText(deviceXSeries.IsChecksumValid, "Valid", "Invalid"));

        }

        Output.WriteVerboseLine("  EEPROM");
        var eepromBytes = device.GetEepromBytes();
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
            Output.WriteVerboseLine("    " + sbHex.ToString() + " " + sbAscii.ToString());
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
