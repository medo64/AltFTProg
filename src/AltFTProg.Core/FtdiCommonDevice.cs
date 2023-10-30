namespace AltFTProg;
using System;

/// <summary>
/// Common FTDI device.
/// All functionality that seems to be common to majority of FTDI devices.
/// </summary>
public abstract class FtdiCommonDevice : FtdiDevice {

    internal FtdiCommonDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, int eepromSize)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes, eepromSize) {
    }


    /// <summary>
    /// Gets/sets remote wakeup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override bool IsRemoteWakeupEnabled {
        get { return base.IsRemoteWakeupEnabled; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[8] = (byte)((EepromBytes[8] & ~0x20) | (value ? 0x20 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if device is self-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override bool IsSelfPowered {
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
    public override int MaxPower {  // 2 mA unit
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
    /// Gets/sets if IO pins will be pulled down in USB suspend.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public override bool IsIOPulledDownDuringSuspend {
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
    public override bool IsSerialNumberEnabled {
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

}
