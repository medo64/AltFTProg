namespace AltFTProg;
using System;
using System.Security.Cryptography;

/// <summary>
/// FTDI X series device.
/// </summary>
public sealed class FtdiXSeriesDevice : FtdiCommonDevice {

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
    public override string ProductDescription {
        get { return base.ProductDescription; }
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
    public override string SerialNumber {
        get { return base.SerialNumber; }
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
    /// Gets/sets if RS485 echo is supressed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool Rs485EchoSuppression {
        get { return (EepromBytes[0] & 0x08) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x08) | (value ? 0x08 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if D2XX direct driver will be used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool D2xxDirectDriver {
        get { return !VirtualComPortDriver; }
        set { VirtualComPortDriver = !value; }
    }

    /// <summary>
    /// Gets/sets if Virtual COM port driver will be used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool VirtualComPortDriver {
        get { return (EepromBytes[0] & 0x80) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x80) | (value ? 0x80 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if Battery charge is enabled.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool BatteryChargeEnable {
        get { return (EepromBytes[0] & 0x01) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x01) | (value ? 0x01 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if Power enable is forced.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool ForcePowerEnable {
        get { return (EepromBytes[0] & 0x02) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x02) | (value ? 0x02 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if sleep is deactivated.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DeactivateSleep {
        get { return (EepromBytes[0] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus0Signal {
        get { return (CBusPinSignal)(EepromBytes[26]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[26] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus1Signal {
        get { return (CBusPinSignal)(EepromBytes[27]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[27] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS2.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus2Signal {
        get { return (CBusPinSignal)(EepromBytes[28]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[28] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus3Signal {
        get { return (CBusPinSignal)(EepromBytes[29]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[29] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if if DBUS slow slew is used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DBusSlowSlew {
        get { return (EepromBytes[12] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[12] = (byte)((EepromBytes[12] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets DBUS drive current.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported drive current value (must be either 4, 8, 12, or 16).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public int DBusDriveCurrent {
        get { return ((EepromBytes[12] & 0x03) + 1) * 4; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value is not (4 or 8 or 12 or 16)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported drive current value (must be either 4, 8, 12, or 16)."); }
            EepromBytes[12] = (byte)((EepromBytes[12] & ~0x03) | ((value / 4) - 1));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DBUS uses schmitt input.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DBusSchmittInput {
        get { return (EepromBytes[12] & 0x08) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[12] = (byte)((EepromBytes[12] & ~0x08) | (value ? 0x08 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if if DBUS slow slew is used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool CBusSlowSlew {
        get { return (EepromBytes[12] & 0x40) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[12] = (byte)((EepromBytes[12] & ~0x40) | (value ? 0x40 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets CBUS drive current.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported drive current value (must be either 4, 8, 12, or 16).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public int CBusDriveCurrent {
        get { return (((EepromBytes[12] & 0x30) >> 4) + 1) * 4; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value is not (4 or 8 or 12 or 16)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported drive current value (must be either 4, 8, 12, or 16)."); }
            EepromBytes[12] = (byte)((EepromBytes[12] & ~0x30) | (((value / 4) - 1) << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if CBUS uses schmitt input.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool CBusSchmittInput {
        get { return (EepromBytes[12] & 0x80) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[12] = (byte)((EepromBytes[12] & ~0x80) | (value ? 0x80 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Resets device to default configuration.
    /// </summary>
    public override void ResetEepromToDefaults() {
        var defaultEepromHex =
        @"
            01 00 03 04 15 60 00 10  80 32 08 00 44 00 A0 0A
            AA 32 DC 12 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            2E 36 D1 C9 01 00 4B 82  AF A0 40 00 00 00 00 00
            00 00 00 00 44 41 58 4D  53 31 32 35 00 00 00 00
            0A 03 46 00 54 00 44 00  49 00 32 03 55 00 53 00
            42 00 20 00 3C 00 2D 00  3E 00 20 00 53 00 65 00
            72 00 69 00 61 00 6C 00  20 00 43 00 6F 00 6E 00
            76 00 65 00 72 00 74 00  65 00 72 00 12 03 46 00
            54 00 38 00 43 00 58 00  57 00 34 00 55 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 F8 60
        ";
        var defaultEepromBytes = Helpers.HexStringToByteArray(defaultEepromHex);
        Buffer.BlockCopy(defaultEepromBytes, 0, EepromBytes, 0, defaultEepromBytes.Length);

        // serial
        var digitCount = 6;
        var rndBytes = RandomNumberGenerator.GetBytes(digitCount);
        for (var i = 0; i < digitCount; i++) {
            var number = rndBytes[i] % 32;
            var ch = (number < 26) ? (char)('A' + number) : (char)('2' + (number - 26));
            EepromBytes[0xE2 + i * 2] = (byte)ch;
        }

        IsChecksumValid = true;  // fixup checksum
    }



    /// <summary>
    /// FTDI X Series CBUS pin signal.
    /// </summary>
    public enum CBusPinSignal {
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
        /// TX&amp;RXLED# function.
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
