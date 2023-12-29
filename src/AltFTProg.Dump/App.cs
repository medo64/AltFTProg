namespace AltFTProgDump;
using System;
using System.Globalization;
using System.Runtime.InteropServices.Marshalling;
using AltFTProg;

internal static class App {
    internal static void Main(string[] args) {
        var envUser = Environment.GetEnvironmentVariable("USER") ?? "";
        bool isRoot = envUser.Equals("root", StringComparison.Ordinal);
        if (!isRoot) {
            Console.WriteLine("This program must be run as root.");
            Environment.Exit(1);
        }

        var vidPids = new KeyValuePair<int, int>[] {
            new(0x0000, 0x0000),
            new(0x0403, 0x6001),
            new(0x0403, 0x6010),
            new(0x0403, 0x6011),
            new(0x0403, 0x6014),
            new(0x0403, 0x6015),
        };

        foreach (var vidPid in vidPids) {
            var devices = FtdiDevice.GetDevices(vidPid.Key, vidPid.Value);
            foreach (var device in devices) {
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(device.DeviceType.ToString());
                Console.ResetColor();
                Console.WriteLine("  "
                                + device.UsbVendorId.ToString("X4", CultureInfo.InvariantCulture)
                                + ":"
                                + device.UsbProductId.ToString("X4", CultureInfo.InvariantCulture)
                                + " \""
                                + device.UsbManufacturer
                                + "\" \""
                                + device.UsbProductDescription
                                + "\""
                        );
                var bytes = device.GetEepromBytes();
                for (var i = 0; i < bytes.Length; i += 16) {
                    Console.Write("  ");
                    for (var j = 0; j < 16; j++) {
                        var b = bytes[i + j];
                        Console.Write(b.ToString("X2", CultureInfo.InvariantCulture));
                        if ((j % 8) == 7) { Console.Write("  "); } else { Console.Write(" "); }
                    }
                    for (var j = 0; j < 16; j++) {
                        var b = bytes[i + j];
                        if (b is < 32 or > 126) {
                            Console.Write("·");
                        } else {
                            Console.Write((char)b);
                        }
                        if ((j % 8) == 7) { Console.Write(" "); }
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
