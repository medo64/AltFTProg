namespace AltFTProg;
using System;
using System.Linq;
using System.Text;

internal static class App {

    internal static void Main() {
        //var ftdi = new Ftdi.ftdi_context();
        //var x = Ftdi.ftdi_init(ref ftdi);

        //Console.WriteLine($"{x} {ftdi.usb_dev}");
        //Console.WriteLine("Hello, World!");

        var devs = FtdiDevice.GetDevices();
        foreach (var dev in devs) {
            Console.WriteLine("Device");
            Console.WriteLine("  Manufacturer: " + dev.Manufacturer);
            Console.WriteLine("  Description : " + dev.Description);
            Console.WriteLine("  Serial .....: " + dev.Serial);

            Console.WriteLine("  EEPROM");
            var eepromBytes = dev.GetRawEepromBytes();
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
