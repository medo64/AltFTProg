namespace AltFTProg;
using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Common FTDI device.
/// All functionality that seems to be common to majority of FTDI devices.
/// </summary>
public abstract class FtdiCommonDevice : FtdiDevice {

    internal FtdiCommonDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, int eepromSize)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes, eepromSize) {
    }


    /// <summary>
    /// Gets/sets if device is self-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override bool SelfPowered {
        get { return (EepromBytes[8] & 0x40) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[8] = (byte)((EepromBytes[8] & ~0x40) | (value ? 0x40 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device power requirement.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Value must be between 0 and 500.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override int MaxBusPower {  // 2 mA unit
        get { return EepromBytes[9] * 2; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value is < 0 or > 500) { throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 500."); }
            var newValue = (value + 1) / 2;  // round up
            EepromBytes[9] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets remote wakeup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override bool RemoteWakeupEnabled {
        get { return base.RemoteWakeupEnabled; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[8] = (byte)((EepromBytes[8] & ~0x20) | (value ? 0x20 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if IO pins will be pulled down in USB suspend.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override bool PulldownPinsInSuspend {
        get { return (EepromBytes[10] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[10] = (byte)((EepromBytes[10] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if serial number will be reported by device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override bool SerialNumberEnabled {
        get { return (EepromBytes[10] & 0x08) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[10] = (byte)((EepromBytes[10] & ~0x08) | (value ? 0x08 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if TXD is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool TxdInverted {
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
    public bool RxdInverted {
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
    public bool RtsInverted {
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
    public bool CtsInverted {
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
    public bool DtrInverted {
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
    public bool DsrInverted {
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
    public bool DcdInverted {
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
    public bool RiInverted {
        get { return (EepromBytes[11] & 0x80) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x80) | (value ? 0x80 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Returns random serial number.
    /// </summary>
    /// <param name="prefix">Prefix text.</param>
    /// <param name="digitCount">Number of random digits</param>
    /// <exception cref="ArgumentNullException">Prefix is not null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Digit count must be larger than 0.</exception>
    public static string GetRandomSerialNumber(string prefix, int digitCount) {
        if (prefix == null) { throw new ArgumentNullException(nameof(prefix), "Prefix is not null."); }
        if (digitCount < 1) { throw new ArgumentOutOfRangeException(nameof(digitCount), "Digit count must be larger than 0."); }

        var rndBytes = RandomNumberGenerator.GetBytes(digitCount);
        var sb = new StringBuilder(prefix);
        for (var i = 0; i < digitCount; i++) {
            var number = rndBytes[i] % 32;
            var ch = (number < 26) ? (char)('A' + number) : (char)('2' + (number - 26));
            sb.Append(ch);
        }
        return sb.ToString();
    }


}
