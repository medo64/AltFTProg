namespace AltFTProg;
using System;

/// <summary>
/// FTDI X series device.
/// </summary>
public sealed class FtdiXSeriesDevice : FtdiDevice {

    internal FtdiXSeriesDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, int eepromSize)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes, eepromSize) {
    }


    /// <summary>
    /// Gets/sets device manufacturer name.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 44 characters.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override string Manufacturer {
        get { return base.Manufacturer; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            Helpers.GetEepromStrings(EepromBytes, EepromSize, out _, out var product, out var serial);
            if (Helpers.CountUnicodeChars(value, product, serial) > 44 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Combined length of manufacturer, product, and serial USB string can be up to 44 characters."); }
            Helpers.SetEepromStrings(EepromBytes, EepromSize, value, product, serial);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device product name.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 44 characters.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override string Product {
        get { return base.Product; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            Helpers.GetEepromStrings(EepromBytes, EepromSize, out var manufacturer, out _, out var serial);
            if (Helpers.CountUnicodeChars(manufacturer, value, serial) > 44 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Combined length of manufacturer, product, and serial USB string can be up to 44 characters."); }
            Helpers.SetEepromStrings(EepromBytes, EepromSize, manufacturer, value, serial);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device serial number.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Serial USB string can be up to 15 characters. -or- Combined length of manufacturer, product, and serial USB string can be up to 44 characters.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override string Serial {
        get { return base.Serial; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            if (Helpers.CountUnicodeChars(value) > 15 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Serial USB string can be up to 15 characters."); }

            Helpers.GetEepromStrings(EepromBytes, EepromSize, out var manufacturer, out var product, out _);
            if (Helpers.CountUnicodeChars(manufacturer, product, value) > 44 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Combined length of manufacturer, product, and serial USB string can be up to 44 characters."); }
            Helpers.SetEepromStrings(EepromBytes, EepromSize, manufacturer, product, value);
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinFunction CBus0Function {
        get { return (CBusPinFunction)(EepromBytes[26]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBusPinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[26] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinFunction CBus1Function {
        get { return (CBusPinFunction)(EepromBytes[27]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBusPinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[27] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS2.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinFunction CBus2Function {
        get { return (CBusPinFunction)(EepromBytes[28]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBusPinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[28] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets function for CBUS3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinFunction CBus3Function {
        get { return (CBusPinFunction)(EepromBytes[29]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (Enum.IsDefined(typeof(CBusPinFunction), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[29] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }



    /// <summary>
    /// FTDI X Series CBUS pin function.
    /// </summary>
    public enum CBusPinFunction {
        /// <summary>
        /// Tristate function.
        /// </summary>
        Tristate = 0,

        /// <summary>
        /// RXLED# function.
        /// </summary>
        RxLed = 1,

        /// <summary>
        /// TXLED# function.
        /// </summary>
        TxLed = 2,

        /// <summary>
        /// TX&RXLED# function.
        /// </summary>
        TxRxLed = 3,

        /// <summary>
        /// PWREN# function.
        /// </summary>
        PwrEn = 4,

        /// <summary>
        /// SLEEP# function.
        /// </summary>
        Sleep = 5,

        /// <summary>
        /// Drive_0 function.
        /// </summary>
        Drive0 = 6,

        /// <summary>
        /// Drive_1 function.
        /// </summary>
        Drive1 = 7,

        /// <summary>
        /// GPIO function.
        /// </summary>
        Gpio = 8,

        /// <summary>
        /// TXDEN function.
        /// </summary>
        TxdEn = 9,

        /// <summary>
        /// CLK24 function.
        /// </summary>
        Clock24Mhz = 10,

        /// <summary>
        /// CLK12 function.
        /// </summary>
        Clock12Mhz = 11,

        /// <summary>
        /// CLK6 function.
        /// </summary>
        Clock6Mhz = 12,

        /// <summary>
        /// BCD_Charger function.
        /// </summary>
        BcdCharger = 13,

        /// <summary>
        /// BCD_Charger# function.
        /// </summary>
        BcdChargerN = 14,

        /// <summary>
        /// I2C_TXE# function.
        /// </summary>
        I2CTxE = 15,

        /// <summary>
        /// I2C_RXF# function.
        /// </summary>
        I2CRxF = 16,

        /// <summary>
        /// VBUS_Sense function.
        /// </summary>
        VbusSense = 17,

        /// <summary>
        /// BitBang WR# function.
        /// </summary>
        BitBangWr = 18,

        /// <summary>
        /// BitBang RD# function.
        /// </summary>
        BitBangRd = 19,

        /// <summary>
        /// Time_Stamp function.
        /// </summary>
        TimeStamp = 20,

        /// <summary>
        /// Keep_Awake# function.
        /// </summary>
        KeepAwake = 21,
    }

}
