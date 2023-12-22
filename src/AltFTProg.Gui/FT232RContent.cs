namespace AltFTProgGui;
using Avalonia.Controls;
using AltFTProg;

internal class FT232RContent {

    public FT232RContent(Ftdi232RDevice device) {
        Device = device;
    }

    private readonly Ftdi232RDevice Device;


    public void Populate(TabControl Tabs) {
        {
            var tab = FTContent.NewTab("USB", out var grid);
            PopulateUsb(grid);
            Tabs.Items.Add(tab);
        }
        {
            var tab = FTContent.NewTab("IO", out var grid);
            PopulateIO(grid);
            Tabs.Items.Add(tab);
        }
        {
            var tab = FTContent.NewTab("Hardware", out var grid);
            PopulateHardware(grid);
            Tabs.Items.Add(tab);
        }
    }

    private void PopulateUsb(Grid grid) {
        FTContent.NewTextRow(grid,  "Vendor ID",                   Device.VendorId.ToString("X4"),
            isEnabled: false
        );
        FTContent.NewTextRow(grid,  "Product ID",                  Device.ProductId.ToString("X4"),
            isEnabled: false
        );

        FTContent.NewSeparatorRow(grid);

        FTContent.NewTextRow(grid,  "Manufacturer",                Device.Manufacturer,
            apply: (value) => { Device.Manufacturer = value; }
        );
        FTContent.NewTextRow(grid,  "Product description",         Device.ProductDescription,
            apply: (value) => { Device.ProductDescription = value; }
        );

        FTContent.NewSeparatorRow(grid);

        FTContent.NewTextRow(grid,  "Serial number",               Device.SerialNumber,
            apply: (value) => { Device.SerialNumber = value; },
            button: () => {
                var prefix = "FT";
                var digitCount = 6;
                return FtdiCommonDevice.GetRandomSerialNumber(prefix, digitCount);
            }
        ); // TODO: ask for prefix and length

        FTContent.NewCheckRow(grid,  "Serial number enabled",       Device.SerialNumberEnabled,
            apply: (value) => { Device.SerialNumberEnabled = value; }
        );

        FTContent.NewSeparatorRow(grid);

        FTContent.NewCheckRow(grid, "Remote wakeup",               Device.RemoteWakeupEnabled,
            apply: (value) => { Device.RemoteWakeupEnabled = value; }
        );
        FTContent.NewCheckRow(grid, "Bus powered",                 Device.BusPowered,
            apply: (value) => { Device.BusPowered = value; }
        );
    }

    private void PopulateIO(Grid grid) {
        FTContent.NewComboRow(grid, "CBUS 0 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 1 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 2 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 3 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 4 Function",             "");
    }

    private void PopulateHardware(Grid grid) {
        FTContent.NewCheckRow(grid, "High-current I/O",            Device.HighCurrentIO,
            apply: (value) => { Device.HighCurrentIO = value; }
        );
        FTContent.NewCheckRow(grid, "Use D2XX driver",             Device.D2xxDirectDriver,
            apply: (value) => { Device.D2xxDirectDriver = value; }
        );
        FTContent.NewCheckRow(grid, "External oscillator",         Device.ExternalOscillator,
            isEnabled: false
        );

        FTContent.NewSeparatorRow(grid);

        FTContent.NewCheckRow(grid, "Invert TXD",                  Device.TxdInverted,
            apply: (value) => { Device.TxdInverted = value; }
        );
        FTContent.NewCheckRow(grid, "Invert RXD",                  Device.RxdInverted,
            apply: (value) => { Device.RxdInverted = value; }
        );
        FTContent.NewCheckRow(grid, "Invert RTS",                  Device.RtsInverted,
            apply: (value) => { Device.RtsInverted = value; }
        );
        FTContent.NewCheckRow(grid, "Invert CTS",                  Device.CtsInverted,
            apply: (value) => { Device.CtsInverted = value; }
        );
        FTContent.NewCheckRow(grid, "Invert DTR",                  Device.DtrInverted,
            apply: (value) => { Device.DtrInverted = value; }
        );
        FTContent.NewCheckRow(grid, "Invert DSR",                  Device.DsrInverted,
            apply: (value) => { Device.DsrInverted = value; }
        );
        FTContent.NewCheckRow(grid, "Invert DCD",                  Device.DcdInverted,
            apply: (value) => { Device.DcdInverted = value; }
        );
        FTContent.NewCheckRow(grid, "Invert RI",                   Device.RiInverted,
            apply: (value) => { Device.RiInverted = value; }
        );
    }

}
