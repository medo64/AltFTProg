﻿namespace AltFTProg;
using System;
using System.Diagnostics;
using System.Text;

internal static class App {

    internal static void Main() {
        var devs = FtdiDevice.GetDevices();
        foreach (var dev in devs) {
            Console.WriteLine("FTDI Device");
            Console.WriteLine("  Manufacturer ........: " + dev.Manufacturer);
            Console.WriteLine("  Product .............: " + dev.Product);
            Console.WriteLine("  Serial ..............: " + dev.Serial);

            Console.WriteLine("  Vendor ID ...........: 0x" + dev.VendorId.ToString("X4"));
            Console.WriteLine("  Product ID ..........: 0x" + dev.ProductId.ToString("X4"));
            Console.WriteLine("  Remote wakeup .......: " + (dev.IsRemoteWakeupEnabled ? "Enabled" : "Disabled"));
            Console.WriteLine("  Power source ........: " + (dev.IsSelfPowered ? "Self-powered" : "Bus-powered"));
            Console.WriteLine("  Maximum power .......: " + dev.MaxPower.ToString() + " mA");
            Console.WriteLine("  IO during suspend ...: " + (dev.IsIOPulledDownDuringSuspend ? "Pulled-down" : "Floating"));
            Console.WriteLine("  Serial number enabled: " + (dev.IsSerialNumberEnabled ? "Yes" : "No"));
            Console.WriteLine("  USB .................: " + (dev.IsUsb11 ? "1.1" : "2.0"));
            Console.WriteLine("  TXD inverted ........: " + (dev.IsTxdInverted ? "Yes" : "No"));
            Console.WriteLine("  RXD inverted ........: " + (dev.IsRxdInverted ? "Yes" : "No"));
            Console.WriteLine("  RTS inverted ........: " + (dev.IsRtsInverted ? "Yes" : "No"));
            Console.WriteLine("  CTS inverted ........: " + (dev.IsCtsInverted ? "Yes" : "No"));
            Console.WriteLine("  DTR inverted ........: " + (dev.IsDtrInverted ? "Yes" : "No"));
            Console.WriteLine("  DSR inverted ........: " + (dev.IsDsrInverted ? "Yes" : "No"));
            Console.WriteLine("  DCD inverted ........: " + (dev.IsDcdInverted ? "Yes" : "No"));
            Console.WriteLine("  RI inverted .........: " + (dev.IsRiInverted ? "Yes" : "No"));
            Console.WriteLine("  High-current IO .....: " + (dev.IsHighCurrentIO ? "Yes" : "No"));
            Console.WriteLine("  Checksum ............: " + (dev.IsChecksumValid ? "Valid" : "Invalid"));

            Console.WriteLine("  EEPROM");
            var eepromBytes = dev.GetEepromBytes(includeExtras: true);
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
                    sbAscii.Append((b < 32 || b > 126) ? '·' : (char)b);
                }
                Console.WriteLine("    " + sbHex.ToString() + " " + sbAscii.ToString());
            }
        }
    }
}
