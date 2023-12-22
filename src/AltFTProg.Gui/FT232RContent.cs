namespace AltFTProgGui;
using Avalonia.Controls;
using AltFTProg;

internal class FT232RContent(Ftdi232RDevice Device) {

    public void Populate(TabControl Tabs) {
        {  // USB
            var tab = FTContent.NewTab("USB", out var grid);

            FTContent.NewHexRow(grid, "Vendor ID",                      Device.VendorId.ToString("X4"),
                isEnabled: false,  // TODO: allow change in settings
                validate: (value) => { return value is >= 0 and <= 65535; }
            );
            FTContent.NewHexRow(grid, "Product ID",                     Device.ProductId.ToString("X4"),
                isEnabled: false,  // TODO: allow change in settings
                validate: (value) => { return value is >= 0 and <= 65535; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewStringRow(grid, "Manufacturer",                Device.Manufacturer,
                apply: (value) => { Device.Manufacturer = value; }
            );
            FTContent.NewStringRow(grid, "Product description",         Device.ProductDescription,
                apply: (value) => { Device.ProductDescription = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewStringRow(grid, "Serial number",               Device.SerialNumber,
                apply: (value) => { Device.SerialNumber = value; },
                button: () => {
                    var prefix = "FT";
                    var digitCount = 6;
                    return FtdiCommonDevice.GetRandomSerialNumber(prefix, digitCount);
                }
            ); // TODO: ask for prefix and length

            FTContent.NewBooleanRow(grid, "Serial number enabled",      Device.SerialNumberEnabled,
                apply: (value) => { Device.SerialNumberEnabled = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid,"Remote wakeup",               Device.RemoteWakeupEnabled,
                apply: (value) => { Device.RemoteWakeupEnabled = value; }
            );
            FTContent.NewBooleanRow(grid,"Bus powered",                 Device.BusPowered,
                apply: (value) => { Device.BusPowered = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // IO
            var tab = FTContent.NewTab("IO", out var grid);

            FTContent.NewEnumRow(grid,"CBUS 0 Function",                "");
            FTContent.NewEnumRow(grid,"CBUS 1 Function",                "");
            FTContent.NewEnumRow(grid,"CBUS 2 Function",                "");
            FTContent.NewEnumRow(grid,"CBUS 3 Function",                "");
            FTContent.NewEnumRow(grid,"CBUS 4 Function",                "");

            Tabs.Items.Add(tab);
        }

        {  // Hardware
            var tab = FTContent.NewTab("Hardware", out var grid);

            FTContent.NewBooleanRow(grid, "High-current I/O",           Device.HighCurrentIO,
                apply: (value) => { Device.HighCurrentIO = value; }
            );
            FTContent.NewBooleanRow(grid, "Use D2XX driver",            Device.D2xxDirectDriver,
                apply: (value) => { Device.D2xxDirectDriver = value; }
            );
            FTContent.NewBooleanRow(grid, "External oscillator",        Device.ExternalOscillator,
                isEnabled: false
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid, "Invert TXD",                 Device.TxdInverted,
                apply: (value) => { Device.TxdInverted = value; }
            );
            FTContent.NewBooleanRow(grid, "Invert RXD",                 Device.RxdInverted,
                apply: (value) => { Device.RxdInverted = value; }
            );
            FTContent.NewBooleanRow(grid, "Invert RTS",                 Device.RtsInverted,
                apply: (value) => { Device.RtsInverted = value; }
            );
            FTContent.NewBooleanRow(grid, "Invert CTS",                 Device.CtsInverted,
                apply: (value) => { Device.CtsInverted = value; }
            );
            FTContent.NewBooleanRow(grid, "Invert DTR",                 Device.DtrInverted,
                apply: (value) => { Device.DtrInverted = value; }
            );
            FTContent.NewBooleanRow(grid, "Invert DSR",                 Device.DsrInverted,
                apply: (value) => { Device.DsrInverted = value; }
            );
            FTContent.NewBooleanRow(grid, "Invert DCD",                 Device.DcdInverted,
                apply: (value) => { Device.DcdInverted = value; }
            );
            FTContent.NewBooleanRow(grid, "Invert RI",                  Device.RiInverted,
                apply: (value) => { Device.RiInverted = value; }
            );

            Tabs.Items.Add(tab);
        }
    }

}
