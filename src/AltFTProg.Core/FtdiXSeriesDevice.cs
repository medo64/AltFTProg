namespace AltFTProg;
using System;
using System.Security.Cryptography;

/// <summary>
/// FTDI X series device.
/// </summary>
public sealed class FtdiXSeriesDevice : FtdiCommonDevice {

    internal FtdiXSeriesDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes,
            new EepromStrings(
                eepromBytes,
                pointersOffset: 0x0E,
                pointersOffsetMask: 0xFF,
                dataOffset: 0xA0,
                dataLength: 92 + 2  // 92 bytes officially, but FT_Prog also uses 2 reserved bytes
            )
        ) {
    }


    #region Misc Config @ 0x00 - 0x01

    /// <summary>
    /// Gets/sets if Battery charge is enabled.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool BatteryChargeEnable {
        get { return (EepromBytes[0x00] & 0x01) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x01) | (value ? 0x01 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if Power enable is forced.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool ForcePowerEnable {
        get { return (EepromBytes[0x00] & 0x02) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x02) | (value ? 0x02 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if sleep is deactivated.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DeactivateSleep {
        get { return (EepromBytes[0x00] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x04) | (value ? 0x04 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RS485 echo is supressed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool Rs485EchoSuppression {
        get { return (EepromBytes[0x00] & 0x08) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x08) | (value ? 0x08 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if external oscillator will be used.
    /// </summary>
    public override bool ExternalOscillator {
        get { return (EepromBytes[0x00] & 0x10) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x10) | (value ? 0x10 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if external oscillator has feedback resistor enabled.
    /// </summary>
    public bool ExternalOscillatorFeedbackResistor {
        get { return (EepromBytes[0x00] & 0x20) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x20) | (value ? 0x20 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets if a CBUS pin has been allocated to VBUS sense mode.
    /// </summary>
    public bool CbusPinVbusSense {
        get { return (EepromBytes[0x00] & 0x40) != 0; }
    }

    #endregion Misc Config @ 0x00 - 0x01


    #region DBUS & CBUS Control @ 0x0C

    /// <summary>
    /// Gets/sets if if DBUS slow slew is used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DBusSlowSlew {
        get { return (EepromBytes[0x0C] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0C] = (byte)((EepromBytes[0x0C] & ~0x04) | (value ? 0x04 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets DBUS drive current.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported drive current value (must be either 4, 8, 12, or 16).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public int DBusDriveCurrent {
        get { return ((EepromBytes[0x0C] & 0x03) + 1) * 4; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value is not (4 or 8 or 12 or 16)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported drive current value (must be either 4, 8, 12, or 16)."); }
            EepromBytes[0x0C] = (byte)((EepromBytes[0x0C] & ~0x03) | ((value / 4) - 1));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DBUS uses schmitt input.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DBusSchmittInput {
        get { return (EepromBytes[0x0C] & 0x08) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0C] = (byte)((EepromBytes[0x0C] & ~0x08) | (value ? 0x08 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if if DBUS slow slew is used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool CBusSlowSlew {
        get { return (EepromBytes[0x0C] & 0x40) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0C] = (byte)((EepromBytes[0x0C] & ~0x40) | (value ? 0x40 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets CBUS drive current.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported drive current value (must be either 4, 8, 12, or 16).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public int CBusDriveCurrent {
        get { return (((EepromBytes[0x0C] & 0x30) >> 4) + 1) * 4; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value is not (4 or 8 or 12 or 16)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported drive current value (must be either 4, 8, 12, or 16)."); }
            EepromBytes[0x0C] = (byte)((EepromBytes[0x0C] & ~0x30) | (((value / 4) - 1) << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if CBUS uses schmitt input.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool CBusSchmittInput {
        get { return (EepromBytes[0x0C] & 0x80) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0C] = (byte)((EepromBytes[0x0C] & ~0x80) | (value ? 0x80 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion DBUS & CBUS Control @ 0x0C


    #region CBUS Mux Control @ 0x1A - 0x1F

    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus0Signal {
        get { return (CBusPinSignal)(EepromBytes[0x1A]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x1A] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus1Signal {
        get { return (CBusPinSignal)(EepromBytes[0x1B]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x1B] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS2.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus2Signal {
        get { return (CBusPinSignal)(EepromBytes[0x1C]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x1C] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBusPinSignal CBus3Signal {
        get { return (CBusPinSignal)(EepromBytes[0x1D]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBusPinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x1D] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion CBUS Mux Control @ 0x1A - 0x1F


    /// <summary>
    /// Resets device to default configuration.
    /// </summary>
    public void ResetEepromToDefaults() {
        var defaultEepromHex =
        @"
            80 00 03 04 15 60 00 10  80 2D 08 00 00 00 A0 0A
            AA 24 D4 12 00 00 00 00  00 00 09 01 02 05 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            2C 36 D3 C9 01 00 0C 57  76 32 40 00 00 00 00 00
            00 00 00 00 44 33 57 51  58 47 51 39 00 00 00 00
            0A 03 46 00 54 00 44 00  49 00 24 03 46 00 54 00
            32 00 33 00 30 00 58 00  20 00 42 00 61 00 73 00
            69 00 63 00 20 00 55 00  41 00 52 00 54 00 00 00
            00 00 00 00 12 03 44 00  33 00 30 00 49 00 4E 00
            46 00 49 00 37 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 18 2E
        ";
        var defaultEepromBytes = Helpers.HexStringToByteArray(defaultEepromHex);
        Buffer.BlockCopy(defaultEepromBytes, 0, EepromBytes, 0, defaultEepromBytes.Length);

        // serial
        var digitCount = 8;
        var rndBytes = RandomNumberGenerator.GetBytes(digitCount);
        for (var i = 0; i < digitCount; i++) {
            var number = rndBytes[i] % 32;
            var ch = (number < 26) ? (char)('A' + number) : (char)('2' + (number - 26));
            EepromBytes[0xD6 + i * 2] = (byte)ch;
        }

        IsChecksumValid = true;  // fixup checksum
    }


    /// <summary>
    /// Gets/sets if checksum is valid.
    /// If checksum is not valid, it can be made valid by setting the property to true.
    /// </summary>
    /// <exception cref="ArgumentException">Checksum validity cannot be set to false.</exception>
    public override bool IsChecksumValid {
        get {
            var eepromChecksum = (UInt16)(EepromBytes[0xFF] << 8 | EepromBytes[0xFE]);
            var checksum = GetChecksum(EepromBytes);
            return (eepromChecksum == checksum);
        }
        set {
            if (value == false) { throw new ArgumentException("Checksum validity cannot be set to false.", nameof(value)); }
            var checksum = GetChecksum(EepromBytes);
            EepromBytes[0xFF] = (byte)(checksum >> 8);
            EepromBytes[0xFE] = (byte)(checksum & 0xFF);
        }
    }

    private static ushort GetChecksum(byte[] eeprom) {
        UInt16 crc = 0xAAAA;
        for (var i = 0x00; i <= 0xFD; i += 2) {
            if (i == 0x24) { i = 0x7F + 1; }
            crc ^= (UInt16)(eeprom[i] | (eeprom[i + 1] << 8));
            crc = (UInt16)((crc << 1) | (crc >> 15));
        }
        return crc;
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
