namespace AltFTProgGui;
using System;
using Avalonia.Controls;
using AltFTProg;

internal class FTXSeriesContent(FtdiXSeriesDevice Device, Action refreshAction) {

    public void Populate(TabControl Tabs) {
        {  // USB
            var tab = FTContent.NewTab("USB", out var grid);

            FTContent.NewHexRow(refreshAction, grid,
                "Vendor ID",
                value: () => { return Device.VendorId; },
                apply: (value) => { Device.VendorId = (ushort)value; },
                validate: (value) => { return value is >= 0 and <= 65535; },
                isEnabled: false  // TODO: allow change in settings
            );

            FTContent.NewHexRow(refreshAction, grid,
                "Product ID",
                value: () => { return Device.ProductId; },
                apply: (value) => { Device.ProductId = (ushort)value; },
                validate: (value) => { return value is >= 0 and <= 65535; },
                isEnabled: false  // TODO: allow change in settings
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewStringRow(refreshAction, grid,
                "Manufacturer",
                value: () => { return Device.Manufacturer; },
                apply: (value) => { Device.Manufacturer = value; }
            );

            FTContent.NewStringRow(refreshAction, grid,
                "Product description",
                value: () => { return Device.ProductDescription; },
                apply: (value) => { Device.ProductDescription = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewStringRow(refreshAction, grid,
                "Serial number",
                value: () => { return Device.SerialNumber; },
                apply: (value) => { Device.SerialNumber = value; },
                button: () => {
                    var prefix = "FT";
                    var digitCount = 6;
                    return FtdiCommonDevice.GetRandomSerialNumber(prefix, digitCount);
                }
            ); // TODO: ask for prefix and length

            FTContent.NewBooleanRow(refreshAction, grid,
                "Serial number enabled",
                value: () => { return Device.SerialNumberEnabled; },
                apply: (value) => { Device.SerialNumberEnabled = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "Bus powered",
                value: () => { return Device.BusPowered; },
                apply: (value) => { Device.BusPowered = value; }
            );

            FTContent.NewIntegerRow(refreshAction, grid,
                "Maximum bus power",
                value: () => { return Device.MaxBusPower; },
                apply: (value) => { Device.MaxBusPower = value; },
                unit: "mA"
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "Remote wakeup",
                value: () => { return Device.RemoteWakeupEnabled; },
                apply: (value) => { Device.RemoteWakeupEnabled = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // IO
            var tab = FTContent.NewTab("I/O", out var grid);

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(refreshAction, grid,
                "CBUS 0 Function",
                value: () => { return Device.CBus0Signal; },
                apply: (value) => { Device.CBus0Signal = value; }
            );

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(refreshAction, grid,
                "CBUS 1 Function",
                value: () => { return Device.CBus1Signal; },
                apply: (value) => { Device.CBus1Signal = value; }
            );

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(refreshAction, grid,
                "CBUS 2 Function",
                value: () => { return Device.CBus2Signal; },
                apply: (value) => { Device.CBus2Signal = value; }
            );

            FTContent.NewEnumRow<FtdiXSeriesDevice.CBusPinSignal>(refreshAction, grid,
                "CBUS 3 Function",
                value: () => { return Device.CBus3Signal; },
                apply: (value) => { Device.CBus3Signal = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "CBUS Slow-slew",
                value: () => { return Device.CBusSlowSlew; },
                apply: (value) => { Device.CBusSlowSlew = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "CBUS Schmitt",
                value: () => { return Device.CBusSchmittInput; },
                apply: (value) => { Device.CBusSchmittInput = value; }
            );

            FTContent.NewTupleRow<int>(refreshAction, grid,
                "CBUS Drive",
                new (int, string)[] {
                    ( 4,  "4 mA"),
                    ( 8,  "8 mA"),
                    (12, "12 mA"),
                    (16, "16 mA")
                },
                value: () => { return Device.CBusDriveCurrent; },
                apply: (value) => { Device.CBusDriveCurrent = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "DBUS Slow-slew",
                value: () => { return Device.DBusSlowSlew; },
                apply: (value) => { Device.CBusSlowSlew = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "DBUS Schmitt",
                value: () => { return Device.DBusSchmittInput; },
                apply: (value) => { Device.DBusSchmittInput = value; }
            );

            FTContent.NewTupleRow<int>(refreshAction, grid,
                "DBUS Drive",
                new (int, string)[] {
                    ( 4,  "4 mA"),
                    ( 8,  "8 mA"),
                    (12, "12 mA"),
                    (16, "16 mA")
                },
                value: () => { return Device.DBusDriveCurrent; },
                apply: (value) => { Device.DBusDriveCurrent = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "Pull-down I/O in suspend",
                value: () => { return Device.PulldownPinsInSuspend; },
                apply: (value) => { Device.PulldownPinsInSuspend = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // Invert
            var tab = FTContent.NewTab("Invert", out var grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert TXD",
                value: () => { return Device.TxdInverted; },
                apply: (value) => { Device.TxdInverted = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert RXD",
                value: () => { return Device.RxdInverted; },
                apply: (value) => { Device.RxdInverted = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert RTS",
                value: () => { return Device.RtsInverted; },
                apply: (value) => { Device.RtsInverted = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert CTS",
                value: () => { return Device.CtsInverted; },
                apply: (value) => { Device.CtsInverted = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert DTR",
                value: () => { return Device.DtrInverted; },
                apply: (value) => { Device.DtrInverted = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert DSR",
                value: () => { return Device.DsrInverted; },
                apply: (value) => { Device.DsrInverted = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert DCD",
                value: () => { return Device.DcdInverted; },
                apply: (value) => { Device.DcdInverted = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Invert RI",
                value: () => { return Device.RiInverted; },
                apply: (value) => { Device.RiInverted = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // Hardware
            var tab = FTContent.NewTab("Hardware", out var grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "D2XX direct driver",
                value: () => { return Device.D2xxDirectDriver; },
                apply: (value) => { Device.D2xxDirectDriver = value; }
            );

            FTContent.NewSeparatorRow(grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "RS-485 echo supression",
                value: () => { return Device.Rs485EchoSuppression; },
                apply: (value) => { Device.Rs485EchoSuppression = value; }
            );

            Tabs.Items.Add(tab);
        }

        {  // Battery charge
            var tab = FTContent.NewTab("Battery charge", out var grid);

            FTContent.NewBooleanRow(refreshAction, grid,
                "Battery charge enable",
                value: () => { return Device.BatteryChargeEnable; },
                apply: (value) => { Device.BatteryChargeEnable = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Force power enable",
                value: () => { return Device.ForcePowerEnable; },
                apply: (value) => { Device.ForcePowerEnable = value; }
            );

            FTContent.NewBooleanRow(refreshAction, grid,
                "Deactivate sleep",
                value: () => { return Device.DeactivateSleep; },
                apply: (value) => { Device.DeactivateSleep = value; }
            );

            Tabs.Items.Add(tab);
        }
    }

}
