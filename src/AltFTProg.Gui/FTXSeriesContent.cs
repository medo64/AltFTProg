namespace AltFTProgGui;
using Avalonia.Controls;
using AltFTProg;

internal class FTXSeriesContent(FtdiXSeriesDevice Device) {

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

            FTContent.NewBooleanRow(grid,"Bus powered",                 Device.BusPowered,
                apply: (value) => { Device.BusPowered = value; }
            );

            FTContent.NewIntegerRow(grid,"Maximum bus power",           Device.MaxBusPower, "mA",
                apply: (value) => { Device.MaxBusPower = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid,"Remote wakeup",               Device.RemoteWakeupEnabled,
                apply: (value) => { Device.RemoteWakeupEnabled = value; }
            );

            FTContent.NewBooleanRow(grid,"Pull-down I/O in suspend",    Device.PulldownPinsInSuspend,
                apply: (value) => { Device.RemoteWakeupEnabled = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // IO
            var tab = FTContent.NewTab("I/O", out var grid);

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 0 Function", Device.CBus0Signal,
                apply: (value) => { Device.CBus0Signal = value; }
            );
            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 1 Function", Device.CBus1Signal,
                apply: (value) => { Device.CBus1Signal = value; }
            );
            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 2 Function", Device.CBus2Signal,
                apply: (value) => { Device.CBus2Signal = value; }
            );
            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 3 Function", Device.CBus3Signal,
                apply: (value) => { Device.CBus3Signal = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid,"CBUS Slow-slew",              Device.CBusSlowSlew,
                apply: (value) => { Device.CBusSlowSlew = value; }
            );

            FTContent.NewBooleanRow(grid,"CBUS Schmitt",                Device.CBusSchmittInput,
                apply: (value) => { Device.CBusSchmittInput = value; }
            );

            FTContent.NewTupleRow<int>(grid,"CBUS Drive",               Device.CBusDriveCurrent,
                new (int, string)[] {
                    ( 4,  "4 mA"),
                    ( 8,  "8 mA"),
                    (12, "12 mA"),
                    (16, "16 mA")
                },
                apply: (value) => { Device.CBusDriveCurrent = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid,"DBUS Slow-slew",              Device.DBusSlowSlew,
                apply: (value) => { Device.CBusSlowSlew = value; }
            );

            FTContent.NewBooleanRow(grid,"DBUS Schmitt",                Device.DBusSchmittInput,
                apply: (value) => { Device.DBusSchmittInput = value; }
            );

            FTContent.NewTupleRow<int>(grid,"DBUS Drive",               Device.DBusDriveCurrent,
                new (int, string)[] {
                    ( 4,  "4 mA"),
                    ( 8,  "8 mA"),
                    (12, "12 mA"),
                    (16, "16 mA")
                },
                apply: (value) => { Device.DBusDriveCurrent = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // Hardware
            var tab = FTContent.NewTab("Hardware", out var grid);

            FTContent.NewBooleanRow(grid, "Use D2XX driver",            Device.D2xxDirectDriver,
                apply: (value) => { Device.D2xxDirectDriver = value; }
            );
            FTContent.NewBooleanRow(grid, "Virtual COM port driver",    Device.VirtualComPortDriver,
                apply: (value) => { Device.VirtualComPortDriver = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid, "RS-485 echo supression",     Device.Rs485EchoSuppression,
                apply: (value) => { Device.Rs485EchoSuppression = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid, "Battery charge enable",      Device.BatteryChargeEnable,
                apply: (value) => { Device.BatteryChargeEnable = value; }
            );

            FTContent.NewBooleanRow(grid, "Force power enable",         Device.ForcePowerEnable,
                apply: (value) => { Device.ForcePowerEnable = value; }
            );

            FTContent.NewBooleanRow(grid, "Deactivate sleep",           Device.DeactivateSleep,
                apply: (value) => { Device.DeactivateSleep = value; }
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
