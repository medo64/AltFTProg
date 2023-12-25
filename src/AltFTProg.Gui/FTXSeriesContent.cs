namespace AltFTProgGui;
using Avalonia.Controls;
using AltFTProg;
using System.Reflection.Metadata.Ecma335;

internal class FTXSeriesContent(FtdiXSeriesDevice Device) {

    public void Populate(TabControl Tabs) {
        {  // USB
            var tab = FTContent.NewTab("USB", out var grid);

            FTContent.NewHexRow(grid, "Vendor ID",
                value: () => { return Device.VendorId; },
                apply: (value) => { Device.VendorId = (ushort)value; },
                validate: (value) => { return value is >= 0 and <= 65535; },
                isEnabled: false  // TODO: allow change in settings
            );

            FTContent.NewHexRow(grid, "Product ID",
                value: () => { return Device.ProductId; },
                apply: (value) => { Device.ProductId = (ushort)value; },
                validate: (value) => { return value is >= 0 and <= 65535; },
                isEnabled: false  // TODO: allow change in settings
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewStringRow(grid, "Manufacturer",
                value: () => { return Device.Manufacturer; },
                apply: (value) => { Device.Manufacturer = value; }
            );

            FTContent.NewStringRow(grid, "Product description",
                value: () => { return Device.ProductDescription; },
                apply: (value) => { Device.ProductDescription = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewStringRow(grid, "Serial number",
                value: () => { return Device.SerialNumber; },
                apply: (value) => { Device.SerialNumber = value; },
                button: () => {
                    var prefix = "FT";
                    var digitCount = 6;
                    return FtdiCommonDevice.GetRandomSerialNumber(prefix, digitCount);
                }
            ); // TODO: ask for prefix and length

            FTContent.NewBooleanRow(grid, "Serial number enabled",
                value: () => { return Device.SerialNumberEnabled; },
                apply: (value) => { Device.SerialNumberEnabled = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid, "Bus powered",
                value: () => { return Device.BusPowered; },
                apply: (value) => { Device.BusPowered = value; }
            );

            FTContent.NewIntegerRow(grid, "Maximum bus power",
                value: () => { return Device.MaxBusPower; },
                apply: (value) => { Device.MaxBusPower = value; },
                unit: "mA"
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid, "Remote wakeup",
                value: () => { return Device.RemoteWakeupEnabled; },
                apply: (value) => { Device.RemoteWakeupEnabled = value; }
            );

            FTContent.NewBooleanRow(grid, "Pull-down I/O in suspend",
                value: () => { return Device.PulldownPinsInSuspend; },
                apply: (value) => { Device.RemoteWakeupEnabled = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // IO
            var tab = FTContent.NewTab("I/O", out var grid);

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 0 Function",
                value: () => { return Device.CBus0Signal; },
                apply: (value) => { Device.CBus0Signal = value; }
            );

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 1 Function",
                value: () => { return Device.CBus1Signal; },
                apply: (value) => { Device.CBus1Signal = value; }
            );

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 2 Function",
                value: () => { return Device.CBus2Signal; },
                apply: (value) => { Device.CBus2Signal = value; }
            );

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(grid,"CBUS 3 Function",
                value: () => { return Device.CBus3Signal; },
                apply: (value) => { Device.CBus3Signal = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid,"CBUS Slow-slew",
                value: () => { return Device.CBusSlowSlew; },
                apply: (value) => { Device.CBusSlowSlew = value; }
            );

            FTContent.NewBooleanRow(grid,"CBUS Schmitt",
                value: () => { return Device.CBusSchmittInput; },
                apply: (value) => { Device.CBusSchmittInput = value; }
            );

            FTContent.NewTupleRow<int>(grid,"CBUS Drive",
                new (int, string)[] {
                    ( 4,  "4 mA"),
                    ( 8,  "8 mA"),
                    (12, "12 mA"),
                    (16, "16 mA")
                },
                value: () => { return Device.CBusDriveCurrent; },
                apply: (value) => { Device.CBusDriveCurrent = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // Invert
            var tab = FTContent.NewTab("Invert", out var grid);

            FTContent.NewBooleanRow(grid, "Invert TXD",
                value: () => { return Device.TxdInverted; },
                apply: (value) => { Device.TxdInverted = value; }
            );

            FTContent.NewBooleanRow(grid, "Invert RXD",
                value: () => { return Device.RxdInverted; },
                apply: (value) => { Device.RxdInverted = value; }
            );

            FTContent.NewBooleanRow(grid, "Invert RTS",
                value: () => { return Device.RtsInverted; },
                apply: (value) => { Device.RtsInverted = value; }
            );

            FTContent.NewBooleanRow(grid, "Invert CTS",
                value: () => { return Device.CtsInverted; },
                apply: (value) => { Device.CtsInverted = value; }
            );

            FTContent.NewBooleanRow(grid, "Invert DTR",
                value: () => { return Device.DtrInverted; },
                apply: (value) => { Device.DtrInverted = value; }
            );

            FTContent.NewBooleanRow(grid, "Invert DSR",
                value: () => { return Device.DsrInverted; },
                apply: (value) => { Device.DsrInverted = value; }
            );

            FTContent.NewBooleanRow(grid, "Invert DCD",
                value: () => { return Device.DcdInverted; },
                apply: (value) => { Device.DcdInverted = value; }
            );

            FTContent.NewBooleanRow(grid, "Invert RI",
                value: () => { return Device.RiInverted; },
                apply: (value) => { Device.RiInverted = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // Hardware
            var tab = FTContent.NewTab("Hardware", out var grid);

            FTContent.NewBooleanRow(grid, "D2XX direct driver",
                value: () => { return Device.D2xxDirectDriver; },
                apply: (value) => { Device.D2xxDirectDriver = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid, "RS-485 echo supression",
                value: () => { return Device.Rs485EchoSuppression; },
                apply: (value) => { Device.Rs485EchoSuppression = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(grid,"DBUS Slow-slew",
                value: () => { return Device.DBusSlowSlew; },
                apply: (value) => { Device.CBusSlowSlew = value; }
            );

            FTContent.NewBooleanRow(grid,"DBUS Schmitt",
                value: () => { return Device.DBusSchmittInput; },
                apply: (value) => { Device.DBusSchmittInput = value; }
            );

            FTContent.NewTupleRow<int>(grid,"DBUS Drive",
                new (int, string)[] {
                    ( 4,  "4 mA"),
                    ( 8,  "8 mA"),
                    (12, "12 mA"),
                    (16, "16 mA")
                },
                value: () => { return Device.DBusDriveCurrent; },
                apply: (value) => { Device.DBusDriveCurrent = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // Battery charge
            var tab = FTContent.NewTab("Battery charge", out var grid);

            FTContent.NewBooleanRow(grid, "Battery charge enable",
                value: () => { return Device.BatteryChargeEnable; },
                apply: (value) => { Device.BatteryChargeEnable = value; }
            );

            FTContent.NewBooleanRow(grid, "Force power enable",
                value: () => { return Device.ForcePowerEnable; },
                apply: (value) => { Device.ForcePowerEnable = value; }
            );

            FTContent.NewBooleanRow(grid, "Deactivate sleep",
                value: () => { return Device.DeactivateSleep; },
                apply: (value) => { Device.DeactivateSleep = value; }
            );

            Tabs.Items.Add(tab);
        }
    }

}
