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
    /// Gets/sets if TXD is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsTxdInverted {
        get { return (EepromBytes[11] & 0x01) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x01) | (value ? 0x01 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RXD is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsRxdInverted {
        get { return (EepromBytes[11] & 0x02) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x02) | (value ? 0x02 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RTS is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsRtsInverted {
        get {
            return (EepromBytes[11] & 0x04) != 0;
        }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if CTS is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsCtsInverted {
        get {
            return (EepromBytes[11] & 0x08) != 0;
        }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x08) | (value ? 0x08 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DTR is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsDtrInverted {
        get { return (EepromBytes[11] & 0x10) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x10) | (value ? 0x10 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DSR is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsDsrInverted {
        get {
            return (EepromBytes[11] & 0x20) != 0;
        }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x20) | (value ? 0x20 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DCD is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsDcdInverted {
        get { return (EepromBytes[11] & 0x40) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x40) | (value ? 0x40 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RI is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsRiInverted {
        get { return (EepromBytes[11] & 0x80) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x80) | (value ? 0x80 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public PinFunction CBus0Function {
        get { return (PinFunction)(EepromBytes[20] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[20] = (byte)((EepromBytes[20] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public PinFunction CBus1Function {
        get { return (PinFunction)(EepromBytes[20] >> 4); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[20] = (byte)((EepromBytes[20] & 0x0F) | (newValue << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS2.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public PinFunction CBus2Function {
        get { return (PinFunction)(EepromBytes[21] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[21] = (byte)((EepromBytes[21] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public PinFunction CBus3Function {
        get { return (PinFunction)(EepromBytes[21] >> 4); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[21] = (byte)((EepromBytes[21] & 0x0F) | (newValue << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS4.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public PinFunction CBus4Function {
        get { return (PinFunction)(EepromBytes[22] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
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
    /// FTDI FT232R pin function.
    /// </summary>
    public enum PinFunction {
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
        /// BitBang WRn function.
        /// </summary>
        BitbangWrite = 11,

        /// <summary>
        /// BitBang RDn function.
        /// </summary>
        BitbangRead = 12,

        /// <summary>
        /// RXF# function.
        /// </summary>
        RxF = 13,
    }

}
