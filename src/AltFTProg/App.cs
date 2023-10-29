namespace AltFTProg;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
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
            (isVerbose, file) => {
                Run(isVerbose);
            },
            verboseOption, fileArgument);
        rootCommand.Invoke(args);
    }

    private static void Run(bool isVerbose) {
        var devices = FtdiDevice.GetDevices();
        if (devices.Count == 0) {
            Console.Error.WriteLine("No devices found!");
            Environment.Exit(1);
        }

        foreach (var device in devices) {
            if (string.IsNullOrEmpty(device.UsbSerial)) {
                Console.WriteLine("FTDI Device (no serial number)");
            } else {
                Console.WriteLine("FTDI Device (" + device.UsbSerial + ")");
            }
            if (isVerbose) { WriteDeviceDetails(device, includeEepromExtras: true); }
        }
    }

    private static void WriteDeviceDetails(FtdiDevice device, bool includeEepromExtras) {
        Console.WriteLine("  USB Vendor ID .,,....: 0x" + device.UsbVendorId.ToString("X4"));
        Console.WriteLine("  USB Product ID ......: 0x" + device.UsbProductId.ToString("X4"));
        Console.WriteLine("  USB Manufacturer ....: " + device.UsbManufacturer);
        Console.WriteLine("  USB Product .........: " + device.UsbProduct);
        Console.WriteLine("  USB Serial ..........: " + device.UsbSerial);

        Console.WriteLine("  Device type .........: " + GetDeviceTypeText(device));
        Console.WriteLine("  EEPROM size .........: " + device.EepromSize.ToString());

        Console.WriteLine("  Vendor ID ...........: 0x" + device.VendorId.ToString("X4"));
        Console.WriteLine("  Product ID ..........: 0x" + device.ProductId.ToString("X4"));
        Console.WriteLine("  Manufacturer ........: " + device.Manufacturer);
        Console.WriteLine("  Product .............: " + device.Product);
        Console.WriteLine("  Serial ..............: " + device.Serial);
        Console.WriteLine("  Remote wakeup .......: " + GetBooleanText(device.IsRemoteWakeupEnabled, "Enabled", "Disabled"));
        Console.WriteLine("  Power source ........: " + GetBooleanText(device.IsSelfPowered, "Self-powered", "Bus-powered"));
        Console.WriteLine("  Maximum power .......: " + device.MaxPower.ToString() + " mA");
        Console.WriteLine("  IO during suspend ...: " + GetBooleanText(device.IsIOPulledDownDuringSuspend, "Pulled-down", "Floating"));
        Console.WriteLine("  Serial number enabled: " + GetBooleanText(device.IsSerialNumberEnabled, "Yes", "No"));
        if (device is Ftdi232RDevice device232R) {
            Console.WriteLine("  TXD inverted ........: " + GetBooleanText(device232R.IsTxdInverted, "Yes", "No"));
            Console.WriteLine("  RXD inverted ........: " + GetBooleanText(device232R.IsRxdInverted, "Yes", "No"));
            Console.WriteLine("  RTS inverted ........: " + GetBooleanText(device232R.IsRtsInverted, "Yes", "No"));
            Console.WriteLine("  CTS inverted ........: " + GetBooleanText(device232R.IsCtsInverted, "Yes", "No"));
            Console.WriteLine("  DTR inverted ........: " + GetBooleanText(device232R.IsDtrInverted, "Yes", "No"));
            Console.WriteLine("  DSR inverted ........: " + GetBooleanText(device232R.IsDsrInverted, "Yes", "No"));
            Console.WriteLine("  DCD inverted ........: " + GetBooleanText(device232R.IsDcdInverted, "Yes", "No"));
            Console.WriteLine("  RI inverted .........: " + GetBooleanText(device232R.IsRiInverted, "Yes", "No"));
            Console.WriteLine("  CBUS0 function ......: " + GetPinText(device232R.CBus0Function));
            Console.WriteLine("  CBUS1 function ......: " + GetPinText(device232R.CBus1Function));
            Console.WriteLine("  CBUS2 function ......: " + GetPinText(device232R.CBus2Function));
            Console.WriteLine("  CBUS3 function ......: " + GetPinText(device232R.CBus3Function));
            Console.WriteLine("  CBUS4 function ......: " + GetPinText(device232R.CBus4Function));
            Console.WriteLine("  High-current IO .....: " + GetBooleanText(device232R.IsHighCurrentIO, "Yes", "No"));
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

    private static string GetPinText(Ftdi232RDevice.CBus0PinFunction? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus0PinFunction.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus0PinFunction.RxF => "RXF# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus1PinFunction? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus1PinFunction.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus1PinFunction.TxE => "TXE# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus2PinFunction? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus2PinFunction.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus2PinFunction.Rd => "RD# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus3PinFunction? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus3PinFunction.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.IOMode => "IOMODE (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.BitBangWr => "BitBang WR# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.BitBangRd => "BitBang RD# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus3PinFunction.Wr => "WR# (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetPinText(Ftdi232RDevice.CBus4PinFunction? function) {
        return function switch {
            null => "",
            Ftdi232RDevice.CBus4PinFunction.TxdEnable => "TXDEN (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.PowerEnable => "PWREN# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.RxLed => "RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.TxLed => "TXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.TxRxLed => "TX&RXLED# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.Sleep => "SLEEP# (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.Clock48Mhz => "CLK48 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.Clock24Mhz => "CLK24 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.Clock12Mhz => "CLK12 (" + ((int)function).ToString() + ")",
            Ftdi232RDevice.CBus4PinFunction.Clock6Mhz => "CLK6 (" + ((int)function).ToString() + ")",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetBooleanText(bool? value, string textIfTrue, string textIfFalse) {
        if (value == null) { return ""; }
        return value.Value ? textIfTrue : textIfFalse;
    }
}
