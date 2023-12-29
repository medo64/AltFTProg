namespace AltFTProg;
using System;
using System.Collections;
using System.Security.Cryptography;

/// <summary>
/// FTDI 232R device.
/// </summary>
public sealed class Ftdi232RDevice : FtdiCommonDevice {

    internal Ftdi232RDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes,
            new EepromStrings(
                eepromBytes,
                pointersOffset: 0x0E,
                pointersOffsetMask: 0x7F,
                dataOffset: 0x18,
                dataLength: 48 * 2 + 6
            )
        ) {
    }


    #region Misc Config @ 0x00 - 0x01

    /// <summary>
    /// Gets if external oscillator will be used.
    /// </summary>
    public bool ExternalOscillator {
        get { return (EepromBytes[0x00] & 0x02) != 0; }
    }

    /// <summary>
    /// Gets/sets if high-current IO will be used.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool HighCurrentIO {
        get { return (EepromBytes[0x00] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x04) | (value ? 0x04 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion Misc Config @ 0x00 - 0x01


    #region CBUS Mux Control @ 0x14 - 0x16

    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus0PinSignal CBus0Signal {
        get { return (CBus0PinSignal)(EepromBytes[0x14] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBus0PinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x14] = (byte)((EepromBytes[0x14] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus1PinSignal CBus1Signal {
        get { return (CBus1PinSignal)(EepromBytes[0x14] >> 4); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBus1PinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x14] = (byte)((EepromBytes[0x14] & 0x0F) | (newValue << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS2.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus2PinSignal CBus2Signal {
        get { return (CBus2PinSignal)(EepromBytes[0x15] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBus2PinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x15] = (byte)((EepromBytes[0x15] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus3PinSignal CBus3Signal {
        get { return (CBus3PinSignal)(EepromBytes[0x15] >> 4); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBus3PinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x15] = (byte)((EepromBytes[0x15] & 0x0F) | (newValue << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS4.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public CBus4PinSignal CBus4Signal {
        get { return (CBus4PinSignal)(EepromBytes[0x16] & 0x0F); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            var newValue = (int)value;
            if (!Enum.IsDefined(typeof(CBus4PinSignal), newValue)) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[0x16] = (byte)((EepromBytes[0x16] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion CBUS Mux Control @ 0x14 - 0x16


    /// <summary>
    /// Resets device to default configuration.
    /// </summary>
    public void ResetEepromToDefaults() {
        var defaultEepromHex =
        @"
            00 40 03 04 01 60 00 00  A0 2D 08 00 00 00 98 0A
            A2 20 C2 12 23 10 05 00  0A 03 46 00 54 00 44 00
            49 00 20 03 46 00 54 00  32 00 33 00 32 00 52 00
            20 00 55 00 53 00 42 00  20 00 55 00 41 00 52 00
            54 00 12 03 42 00 30 00  00 00 00 00 00 00 00 00
            00 00 00 00 58 E1 40 04  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
            00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00
        ";
        var defaultEepromBytes = Helpers.HexStringToByteArray(defaultEepromHex);
        Buffer.BlockCopy(defaultEepromBytes, 0, EepromBytes, 0, defaultEepromBytes.Length);

        // serial
        var digitCount = 6;
        var rndBytes = RandomNumberGenerator.GetBytes(digitCount);
        for (var i = 0; i < digitCount; i++) {
            var number = rndBytes[i] % 32;
            var ch = (number < 26) ? (char)('A' + number) : (char)('2' + (number - 26));
            EepromBytes[0x48 + i * 2] = (byte)ch;
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
            var eepromChecksum = (UInt16)(EepromBytes[0x7F] << 8 | EepromBytes[0x7E]);
            var checksum = GetChecksum(EepromBytes);
            return (eepromChecksum == checksum);
        }
        set {
            if (value == false) { throw new ArgumentException("Checksum validity cannot be set to false.", nameof(value)); }
            var checksum = GetChecksum(EepromBytes);
            EepromBytes[0x7F] = (byte)(checksum >> 8);
            EepromBytes[0x7E] = (byte)(checksum & 0xFF);
        }
    }

    internal static ushort GetChecksum(byte[] eeprom) {
        UInt16 crc = 0xAAAA;
        for (var i = 0x00; i <= 0x7D; i += 2) {
            crc ^= (UInt16)(eeprom[i] | (eeprom[i + 1] << 8));
            crc = (UInt16)((crc << 1) | (crc >> 15));
        }
        return crc;
    }



    /// <summary>
    /// FTDI FT232R CBUS0 pin function.
    /// </summary>
    public enum CBus0PinSignal {
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
        /// TX&amp;RXLED# function.
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
    public enum CBus1PinSignal {
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
        /// TX&amp;RXLED# function.
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
    public enum CBus2PinSignal {
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
        /// TX&amp;RXLED# function.
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
    public enum CBus3PinSignal {
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
        /// TX&amp;RXLED# function.
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
    public enum CBus4PinSignal {
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
        /// TX&amp;RXLED# function.
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
