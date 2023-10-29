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

        Console.WriteLine("  Device type .........: " + GetDeviceTypeText(device.DeviceType));
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
        Console.WriteLine("  TXD inverted ........: " + GetBooleanText(device.IsTxdInverted, "Yes", "No"));
        Console.WriteLine("  RXD inverted ........: " + GetBooleanText(device.IsRxdInverted, "Yes", "No"));
        Console.WriteLine("  RTS inverted ........: " + GetBooleanText(device.IsRtsInverted, "Yes", "No"));
        Console.WriteLine("  CTS inverted ........: " + GetBooleanText(device.IsCtsInverted, "Yes", "No"));
        Console.WriteLine("  DTR inverted ........: " + GetBooleanText(device.IsDtrInverted, "Yes", "No"));
        Console.WriteLine("  DSR inverted ........: " + GetBooleanText(device.IsDsrInverted, "Yes", "No"));
        Console.WriteLine("  DCD inverted ........: " + GetBooleanText(device.IsDcdInverted, "Yes", "No"));
        Console.WriteLine("  RI inverted .........: " + GetBooleanText(device.IsRiInverted, "Yes", "No"));
        Console.WriteLine("  CBUS0 function ......: " + GetPinText(device.CBus0Function));
        Console.WriteLine("  CBUS1 function ......: " + GetPinText(device.CBus1Function));
        Console.WriteLine("  CBUS2 function ......: " + GetPinText(device.CBus2Function));
        Console.WriteLine("  CBUS3 function ......: " + GetPinText(device.CBus3Function));
        Console.WriteLine("  CBUS4 function ......: " + GetPinText(device.CBus4Function));
        Console.WriteLine("  High-current IO .....: " + GetBooleanText(device.IsHighCurrentIO, "Yes", "No"));
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

    private static string GetDeviceTypeText(FtdiDeviceType type) {
        return type switch {
            FtdiDeviceType.FT232A => "FT232/245AM",
            FtdiDeviceType.FT232B => "FT232/245BM",
            FtdiDeviceType.FT2232D => "FT2232D",
            FtdiDeviceType.FT232R => "FT232R/FT245R",
            FtdiDeviceType.FT2232H => "FT2232H",
            FtdiDeviceType.FT232H => "FT232H",
            FtdiDeviceType.FTXSeries => "FT X Series",
            _ => "(" + ((int)type).ToString() + ")",
        };
    }

    private static string GetPinText(FtdiDevicePinFunction? function) {
        return function switch {
            null => "",
            FtdiDevicePinFunction.TxdEnable => "TXDEN",
            FtdiDevicePinFunction.PowerEnable => "PWREN#",
            FtdiDevicePinFunction.RxLed => "RXLED#",
            FtdiDevicePinFunction.TxLed => "TXLED#",
            FtdiDevicePinFunction.TxRxLed => "TX&RXLED#",
            FtdiDevicePinFunction.Sleep => "SLEEP#",
            FtdiDevicePinFunction.Clock48Mhz => "CLK48",
            FtdiDevicePinFunction.Clock24Mhz => "CLK24",
            FtdiDevicePinFunction.Clock12Mhz => "CLK12",
            FtdiDevicePinFunction.Clock6Mhz => "CLK6",
            FtdiDevicePinFunction.IOMode => "IOMODE",
            FtdiDevicePinFunction.BitbangWrite => "WR#",
            FtdiDevicePinFunction.BitbangRead => "RD#",
            FtdiDevicePinFunction.RxF => "RXF#",
            _ => "(" + ((int)function).ToString() + ")",
        };
    }

    private static string GetBooleanText(bool? value, string textIfTrue, string textIfFalse) {
        if (value == null) { return ""; }
        return value.Value ? textIfTrue : textIfFalse;
    }
}
