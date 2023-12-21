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
        FTContent.NewTextRow(grid,  "Vendor ID",                   Device.VendorId.ToString("X4"));
        FTContent.NewTextRow(grid,  "Product ID",                  Device.ProductId.ToString("X4"));
        FTContent.NewSeparatorRow(grid);
        FTContent.NewTextRow(grid,  "Manufacturer",                Device.Manufacturer);
        FTContent.NewTextRow(grid,  "Product description",         Device.ProductDescription);
        FTContent.NewSeparatorRow(grid);
        FTContent.NewTextRow(grid,  "Serial number",               Device.SerialNumber);
        FTContent.NewTextRow(grid,  "Serial number prefix",        "", isEnabled: false);
        FTContent.NewCheckRow(grid, "Auto-generate serial number", false);
        FTContent.NewSeparatorRow(grid);
        FTContent.NewCheckRow(grid, "Remote wakeup",               Device.RemoteWakeupEnabled);
        FTContent.NewCheckRow(grid, "Bus powered",                 Device.BusPowered);
    }

    private void PopulateIO(Grid grid) {
        FTContent.NewComboRow(grid, "CBUS 0 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 1 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 2 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 3 Function",             "");
        FTContent.NewComboRow(grid, "CBUS 4 Function",             "");
    }

    private void PopulateHardware(Grid grid) {
        FTContent.NewCheckRow(grid, "High-current I/O",            Device.HighCurrentIO);
        FTContent.NewCheckRow(grid, "Use D2XX driver",             Device.D2xxDirectDriver);
        FTContent.NewCheckRow(grid, "External oscillator",         Device.ExternalOscillator,  isEnabled: false);
        FTContent.NewSeparatorRow(grid);
        FTContent.NewCheckRow(grid, "Invert TXD",                  Device.TxdInverted);
        FTContent.NewCheckRow(grid, "Invert RXD",                  Device.RxdInverted);
        FTContent.NewCheckRow(grid, "Invert RTS",                  Device.RtsInverted);
        FTContent.NewCheckRow(grid, "Invert CTS",                  Device.CtsInverted);
        FTContent.NewCheckRow(grid, "Invert DTR",                  Device.DtrInverted);
        FTContent.NewCheckRow(grid, "Invert DSR",                  Device.DsrInverted);
        FTContent.NewCheckRow(grid, "Invert DCD",                  Device.DcdInverted);
        FTContent.NewCheckRow(grid, "Invert RI",                   Device.RiInverted);
    }

}
