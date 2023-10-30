namespace AltFTProg;
using System;

/// <summary>
/// FTDI 232R device.
/// </summary>
public sealed class Ftdi232RDevice : FtdiCommonDevice {

    internal Ftdi232RDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, int eepromSize)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes, eepromSize) {
    }


    /// <summary>
    /// Gets/sets device manufacturer name.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override string Manufacturer {
        get { return base.Manufacturer; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            Helpers.GetEepromStrings(EepromBytes, EepromSize, out _, out var product, out var serial);
            if (Helpers.CountUnicodeChars(value, product, serial) > 48 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Combined length of manufacturer, product, and serial USB string can be up to 48 characters."); }
            Helpers.SetEepromStrings(EepromBytes, EepromSize, value, product, serial);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device product name.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override string Product {
        get { return base.Product; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            Helpers.GetEepromStrings(EepromBytes, EepromSize, out var manufacturer, out _, out var serial);
            if (Helpers.CountUnicodeChars(manufacturer, value, serial) > 48 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Combined length of manufacturer, product, and serial USB string can be up to 48 characters."); }
            Helpers.SetEepromStrings(EepromBytes, EepromSize, manufacturer, value, serial);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device serial number.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Serial USB string can be up to 15 characters. -or- Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override string Serial {
        get { return base.Serial; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (Helpers.CountUnicodeChars(value) > 15 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Serial USB string can be up to 15 characters."); }

            Helpers.GetEepromStrings(EepromBytes, EepromSize, out var manufacturer, out var product, out _);
            if (Helpers.CountUnicodeChars(manufacturer, product, value) > 48 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Combined length of manufacturer, product, and serial USB string can be up to 48 characters."); }
            Helpers.SetEepromStrings(EepromBytes, EepromSize, manufacturer, product, value);
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus0PinFunction CBus0Function {
        get { return (CBus0PinFunction)(EepromBytes[20] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBus0PinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[20] = (byte)((EepromBytes[20] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus1PinFunction CBus1Function {
        get { return (CBus1PinFunction)(EepromBytes[20] >> 4); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBus1PinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[20] = (byte)((EepromBytes[20] & 0x0F) | (newValue << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS2.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus2PinFunction CBus2Function {
        get { return (CBus2PinFunction)(EepromBytes[21] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBus2PinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[21] = (byte)((EepromBytes[21] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus3PinFunction CBus3Function {
        get { return (CBus3PinFunction)(EepromBytes[21] >> 4); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBus3PinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[21] = (byte)((EepromBytes[21] & 0x0F) | (newValue << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS4.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus4PinFunction CBus4Function {
        get { return (CBus4PinFunction)(EepromBytes[22] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBus4PinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[22] = (byte)((EepromBytes[22] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if high-current IO will be used.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsHighCurrentIO {
        get { return (EepromBytes[0] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }



    /// <summary>
    /// FTDI FT232R CBUS0 pin function.
    /// </summary>
    public enum CBus0PinFunction {
        /// <summary>
        /// TXDEN function.
        /// </summary>
        TxdEnable = 0,

        /// <summary>
        /// PWREN# function.
        /// </summary>
        PowerEnable = 1,

        /// <summary>
        /// RXLED# function.
        /// </summary>
        RxLed = 2,

        /// <summary>
        /// TXLED# function.
        /// </summary>
        TxLed = 3,

        /// <summary>
        /// TX&RXLED# function.
        /// </summary>
        TxRxLed = 4,

        /// <summary>
        /// SLEEP# function.
        /// </summary>
        Sleep = 5,

        /// <summary>
        /// CLK48 function.
        /// </summary>
        Clock48Mhz = 6,

        /// <summary>
        /// CLK24 function.
        /// </summary>
        Clock24Mhz = 7,

        /// <summary>
        /// CLK12 function.
        /// </summary>
        Clock12Mhz = 8,

        /// <summary>
        /// CLK6 function.
        /// </summary>
        Clock6Mhz = 9,

        /// <summary>
        /// I/O MODE function.
        /// </summary>
        IOMode = 10,

        /// <summary>
        /// BitBang WR# function.
        /// </summary>
        BitBangWr = 11,

        /// <summary>
        /// BitBang RD# function.
        /// </summary>
        BitBangRd = 12,

        /// <summary>
        /// RXF# function.
        /// </summary>
        RxF = 13,
    }



    /// <summary>
    /// FTDI FT232R CBUS1 pin function.
    /// </summary>
    public enum CBus1PinFunction {
        /// <summary>
        /// TXDEN function.
        /// </summary>
        TxdEnable = 0,

        /// <summary>
        /// PWREN# function.
        /// </summary>
        PowerEnable = 1,

        /// <summary>
        /// RXLED# function.
        /// </summary>
        RxLed = 2,

        /// <summary>
        /// TXLED# function.
        /// </summary>
        TxLed = 3,

        /// <summary>
        /// TX&RXLED# function.
        /// </summary>
        TxRxLed = 4,

        /// <summary>
        /// SLEEP# function.
        /// </summary>
        Sleep = 5,

        /// <summary>
        /// CLK48 function.
        /// </summary>
        Clock48Mhz = 6,

        /// <summary>
        /// CLK24 function.
        /// </summary>
        Clock24Mhz = 7,

        /// <summary>
        /// CLK12 function.
        /// </summary>
        Clock12Mhz = 8,

        /// <summary>
        /// CLK6 function.
        /// </summary>
        Clock6Mhz = 9,

        /// <summary>
        /// I/O MODE function.
        /// </summary>
        IOMode = 10,

        /// <summary>
        /// BitBang WR# function.
        /// </summary>
        BitBangWr = 11,

        /// <summary>
        /// BitBang RD# function.
        /// </summary>
        BitBangRd = 12,

        /// <summary>
        /// TXE# function.
        /// </summary>
        TxE = 13,
    }



    /// <summary>
    /// FTDI FT232R CBUS2 pin function.
    /// </summary>
    public enum CBus2PinFunction {
        /// <summary>
        /// TXDEN function.
        /// </summary>
        TxdEnable = 0,

        /// <summary>
        /// PWREN# function.
        /// </summary>
        PowerEnable = 1,

        /// <summary>
        /// RXLED# function.
        /// </summary>
        RxLed = 2,

        /// <summary>
        /// TXLED# function.
        /// </summary>
        TxLed = 3,

        /// <summary>
        /// TX&RXLED# function.
        /// </summary>
        TxRxLed = 4,

        /// <summary>
        /// SLEEP# function.
        /// </summary>
        Sleep = 5,

        /// <summary>
        /// CLK48 function.
        /// </summary>
        Clock48Mhz = 6,

        /// <summary>
        /// CLK24 function.
        /// </summary>
        Clock24Mhz = 7,

        /// <summary>
        /// CLK12 function.
        /// </summary>
        Clock12Mhz = 8,

        /// <summary>
        /// CLK6 function.
        /// </summary>
        Clock6Mhz = 9,

        /// <summary>
        /// I/O MODE function.
        /// </summary>
        IOMode = 10,

        /// <summary>
        /// BitBang WR# function.
        /// </summary>
        BitBangWr = 11,

        /// <summary>
        /// BitBang RD# function.
        /// </summary>
        BitBangRd = 12,

        /// <summary>
        /// RD# function.
        /// </summary>
        Rd = 13,
    }



    /// <summary>
    /// FTDI FT232R CBUS3 pin function.
    /// </summary>
    public enum CBus3PinFunction {
        /// <summary>
        /// TXDEN function.
        /// </summary>
        TxdEnable = 0,

        /// <summary>
        /// PWREN# function.
        /// </summary>
        PowerEnable = 1,

        /// <summary>
        /// RXLED# function.
        /// </summary>
        RxLed = 2,

        /// <summary>
        /// TXLED# function.
        /// </summary>
        TxLed = 3,

        /// <summary>
        /// TX&RXLED# function.
        /// </summary>
        TxRxLed = 4,

        /// <summary>
        /// SLEEP# function.
        /// </summary>
        Sleep = 5,

        /// <summary>
        /// CLK48 function.
        /// </summary>
        Clock48Mhz = 6,

        /// <summary>
        /// CLK24 function.
        /// </summary>
        Clock24Mhz = 7,

        /// <summary>
        /// CLK12 function.
        /// </summary>
        Clock12Mhz = 8,

        /// <summary>
        /// CLK6 function.
        /// </summary>
        Clock6Mhz = 9,

        /// <summary>
        /// I/O MODE function.
        /// </summary>
        IOMode = 10,

        /// <summary>
        /// BitBang WR# function.
        /// </summary>
        BitBangWr = 11,

        /// <summary>
        /// BitBang RD# function.
        /// </summary>
        BitBangRd = 12,

        /// <summary>
        /// WR# function.
        /// </summary>
        Wr = 13,
    }



    /// <summary>
    /// FTDI FT232R CBUS4 pin function.
    /// </summary>
    public enum CBus4PinFunction {
        /// <summary>
        /// TXDEN function.
        /// </summary>
        TxdEnable = 0,

        /// <summary>
        /// PWREN# function.
        /// </summary>
        PowerEnable = 1,

        /// <summary>
        /// RXLED# function.
        /// </summary>
        RxLed = 2,

        /// <summary>
        /// TXLED# function.
        /// </summary>
        TxLed = 3,

        /// <summary>
        /// TX&RXLED# function.
        /// </summary>
        TxRxLed = 4,

        /// <summary>
        /// SLEEP# function.
        /// </summary>
        Sleep = 5,

        /// <summary>
        /// CLK48 function.
        /// </summary>
        Clock48Mhz = 6,

        /// <summary>
        /// CLK24 function.
        /// </summary>
        Clock24Mhz = 7,

        /// <summary>
        /// CLK12 function.
        /// </summary>
        Clock12Mhz = 8,

        /// <summary>
        /// CLK6 function.
        /// </summary>
        Clock6Mhz = 9,
    }

}
