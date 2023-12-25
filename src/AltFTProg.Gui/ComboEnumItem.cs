namespace AltFTProgGui;
using System;
using AltFTProg;

internal readonly struct ComboEnumItem<T>(T value) where T : struct, Enum {

    public T Value { get; } = value;

    public override readonly string ToString() {
        if (Value is Ftdi232RDevice.CBus0PinSignal cbus0Signal) {
            return ComboEnumItem<T>.ToString(cbus0Signal);
        } else if (Value is Ftdi232RDevice.CBus1PinSignal cbus1Signal) {
            return ComboEnumItem<T>.ToString(cbus1Signal);
        } else if (Value is Ftdi232RDevice.CBus2PinSignal cbus2Signal) {
            return ComboEnumItem<T>.ToString(cbus2Signal);
        } else if (Value is Ftdi232RDevice.CBus3PinSignal cbus3Signal) {
            return ComboEnumItem<T>.ToString(cbus3Signal);
        } else if (Value is Ftdi232RDevice.CBus4PinSignal cbus4Signal) {
            return ComboEnumItem<T>.ToString(cbus4Signal);
        } else if (Value is FtdiXSeriesDevice.CBusPinSignal cbusSignal) {
            return ComboEnumItem<T>.ToString(cbusSignal);
        } else {
            return Value.ToString();
        }
    }

    private static string ToString(Ftdi232RDevice.CBus0PinSignal value) {
        return value switch {
            Ftdi232RDevice.CBus0PinSignal.TxdEnable => "TXD EN",
            Ftdi232RDevice.CBus0PinSignal.PowerEnable => "PWR EN#",
            Ftdi232RDevice.CBus0PinSignal.RxLed => "RX LED#",
            Ftdi232RDevice.CBus0PinSignal.TxLed => "TX LED#",
            Ftdi232RDevice.CBus0PinSignal.TxRxLed => "TX&RX LED#",
            Ftdi232RDevice.CBus0PinSignal.Sleep => "SLEEP#",
            Ftdi232RDevice.CBus0PinSignal.Clock48Mhz => "CLK 48 MHz",
            Ftdi232RDevice.CBus0PinSignal.Clock24Mhz => "CLK 24 MHz",
            Ftdi232RDevice.CBus0PinSignal.Clock12Mhz => "CLK 12 MHz",
            Ftdi232RDevice.CBus0PinSignal.Clock6Mhz => "CLK 6 MHz",
            Ftdi232RDevice.CBus0PinSignal.IOMode => "I/O MODE",
            Ftdi232RDevice.CBus0PinSignal.BitBangWr => "BitBang WR#",
            Ftdi232RDevice.CBus0PinSignal.BitBangRd => "BitBang RD#",
            Ftdi232RDevice.CBus0PinSignal.RxF => "RXF#",
            _ => "(" + ((int)value).ToString() + ")",
        };
    }

    private static string ToString(Ftdi232RDevice.CBus1PinSignal value) {
         return value switch {
            Ftdi232RDevice.CBus1PinSignal.TxdEnable => "TXD EN",
            Ftdi232RDevice.CBus1PinSignal.PowerEnable => "PWR EN#",
            Ftdi232RDevice.CBus1PinSignal.RxLed => "RX LED#",
            Ftdi232RDevice.CBus1PinSignal.TxLed => "TX LED#",
            Ftdi232RDevice.CBus1PinSignal.TxRxLed => "TX&RX LED#",
            Ftdi232RDevice.CBus1PinSignal.Sleep => "SLEEP#",
            Ftdi232RDevice.CBus1PinSignal.Clock48Mhz => "CLK 48 MHz",
            Ftdi232RDevice.CBus1PinSignal.Clock24Mhz => "CLK 24 MHz",
            Ftdi232RDevice.CBus1PinSignal.Clock12Mhz => "CLK 12 MHz",
            Ftdi232RDevice.CBus1PinSignal.Clock6Mhz => "CLK 6 MHz",
            Ftdi232RDevice.CBus1PinSignal.IOMode => "I/O MODE",
            Ftdi232RDevice.CBus1PinSignal.BitBangWr => "BitBang WR#",
            Ftdi232RDevice.CBus1PinSignal.BitBangRd => "BitBang RD#",
            Ftdi232RDevice.CBus1PinSignal.TxE => "TXE#",
            _ => "(" + ((int)value).ToString() + ")",
        };
   }

    private static string ToString(Ftdi232RDevice.CBus2PinSignal value) {
        return value switch {
            Ftdi232RDevice.CBus2PinSignal.TxdEnable => "TXD EN",
            Ftdi232RDevice.CBus2PinSignal.PowerEnable => "PWR EN#",
            Ftdi232RDevice.CBus2PinSignal.RxLed => "RX LED#",
            Ftdi232RDevice.CBus2PinSignal.TxLed => "TX LED#",
            Ftdi232RDevice.CBus2PinSignal.TxRxLed => "TX&RX LED#",
            Ftdi232RDevice.CBus2PinSignal.Sleep => "SLEEP#",
            Ftdi232RDevice.CBus2PinSignal.Clock48Mhz => "CLK 48 MHz",
            Ftdi232RDevice.CBus2PinSignal.Clock24Mhz => "CLK 24 MHz",
            Ftdi232RDevice.CBus2PinSignal.Clock12Mhz => "CLK 12 MHz",
            Ftdi232RDevice.CBus2PinSignal.Clock6Mhz => "CLK 6 MHz",
            Ftdi232RDevice.CBus2PinSignal.IOMode => "I/O MODE",
            Ftdi232RDevice.CBus2PinSignal.BitBangWr => "BitBang WR#",
            Ftdi232RDevice.CBus2PinSignal.BitBangRd => "BitBang RD#",
            Ftdi232RDevice.CBus2PinSignal.Rd => "RD#",
            _ => "(" + ((int)value).ToString() + ")",
        };
    }

    private static string ToString(Ftdi232RDevice.CBus3PinSignal value) {
        return value switch {
            Ftdi232RDevice.CBus3PinSignal.TxdEnable => "TXD EN",
            Ftdi232RDevice.CBus3PinSignal.PowerEnable => "PWR EN#",
            Ftdi232RDevice.CBus3PinSignal.RxLed => "RX LED#",
            Ftdi232RDevice.CBus3PinSignal.TxLed => "TX LED#",
            Ftdi232RDevice.CBus3PinSignal.TxRxLed => "TX&RX LED#",
            Ftdi232RDevice.CBus3PinSignal.Sleep => "SLEEP#",
            Ftdi232RDevice.CBus3PinSignal.Clock48Mhz => "CLK 48 MHz",
            Ftdi232RDevice.CBus3PinSignal.Clock24Mhz => "CLK 24 MHz",
            Ftdi232RDevice.CBus3PinSignal.Clock12Mhz => "CLK 12 MHz",
            Ftdi232RDevice.CBus3PinSignal.Clock6Mhz => "CLK 6 MHz",
            Ftdi232RDevice.CBus3PinSignal.IOMode => "I/O MODE",
            Ftdi232RDevice.CBus3PinSignal.BitBangWr => "BitBang WR#",
            Ftdi232RDevice.CBus3PinSignal.BitBangRd => "BitBang RD#",
            Ftdi232RDevice.CBus3PinSignal.Wr => "WR#",
            _ => "(" + ((int)value).ToString() + ")",
        };
    }

    private static string ToString(Ftdi232RDevice.CBus4PinSignal value) {
         return value switch {
            Ftdi232RDevice.CBus4PinSignal.TxdEnable => "TXD EN",
            Ftdi232RDevice.CBus4PinSignal.PowerEnable => "PWR EN#",
            Ftdi232RDevice.CBus4PinSignal.RxLed => "RX LED#",
            Ftdi232RDevice.CBus4PinSignal.TxLed => "TX LED#",
            Ftdi232RDevice.CBus4PinSignal.TxRxLed => "TX&RX LED#",
            Ftdi232RDevice.CBus4PinSignal.Sleep => "SLEEP#",
            Ftdi232RDevice.CBus4PinSignal.Clock48Mhz => "CLK 48 MHz",
            Ftdi232RDevice.CBus4PinSignal.Clock24Mhz => "CLK 24 MHz",
            Ftdi232RDevice.CBus4PinSignal.Clock12Mhz => "CLK 12 MHz",
            Ftdi232RDevice.CBus4PinSignal.Clock6Mhz => "CLK 6 MHz",
            _ => "(" + ((int)value).ToString() + ")",
        };
   }

    private static string ToString(FtdiXSeriesDevice.CBusPinSignal value) {
        return value switch {
            FtdiXSeriesDevice.CBusPinSignal.Tristate => "Tri-state",
            FtdiXSeriesDevice.CBusPinSignal.RxLed => "RX LED#",
            FtdiXSeriesDevice.CBusPinSignal.TxLed => "TX LED#",
            FtdiXSeriesDevice.CBusPinSignal.TxRxLed => "TX&RX LED#",
            FtdiXSeriesDevice.CBusPinSignal.PwrEn => "PWR EN#",
            FtdiXSeriesDevice.CBusPinSignal.Sleep => "SLEEP#",
            FtdiXSeriesDevice.CBusPinSignal.Drive0 => "Drive 0",
            FtdiXSeriesDevice.CBusPinSignal.Drive1 => "Drive 1",
            FtdiXSeriesDevice.CBusPinSignal.Gpio => "GPIO",
            FtdiXSeriesDevice.CBusPinSignal.TxdEn => "TXD EN",
            FtdiXSeriesDevice.CBusPinSignal.Clock24Mhz => "CLK 24 MHz",
            FtdiXSeriesDevice.CBusPinSignal.Clock12Mhz => "CLK 12 MHz",
            FtdiXSeriesDevice.CBusPinSignal.Clock6Mhz => "CLK 6 MHz",
            FtdiXSeriesDevice.CBusPinSignal.BcdCharger => "BCD Charger",
            FtdiXSeriesDevice.CBusPinSignal.BcdChargerN => "BCD Charger#",
            FtdiXSeriesDevice.CBusPinSignal.I2CTxE => "I2C TXE#",
            FtdiXSeriesDevice.CBusPinSignal.I2CRxF => "I2C RXF#",
            FtdiXSeriesDevice.CBusPinSignal.VbusSense => "VBUS Sense",
            FtdiXSeriesDevice.CBusPinSignal.BitBangWr => "BitBang WR#",
            FtdiXSeriesDevice.CBusPinSignal.BitBangRd => "BitBang RD#",
            FtdiXSeriesDevice.CBusPinSignal.TimeStamp => "Time Stamp",
            FtdiXSeriesDevice.CBusPinSignal.KeepAwake => "Keep Awake",
            _ => "(" + ((int)value).ToString() + ")",
        };
    }

}