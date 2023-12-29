namespace AltFTProg;
using System;
using System.Text;

/// <summary>
/// Common FTDI device.
/// All functionality that seems to be common to majority of FTDI devices.
/// </summary>
public abstract class FtdiCommonDevice : FtdiDevice {

    internal FtdiCommonDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, EepromStrings stringDescriptors)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes) {
        StringDescriptors = stringDescriptors;
    }

    private readonly EepromStrings StringDescriptors;


    #region Misc Config @ 0x00 - 0x01

    /// <summary>
    /// Gets/sets if Virtual COM port driver will be used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool VirtualComPortDriver {
        get { return (EepromBytes[0x00] & 0x08) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x00] = (byte)((EepromBytes[0x00] & ~0x08) | (value ? 0x08 : 0x00));
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

    #endregion Misc Config @ 0x00 - 0x01


    #region USB VID @ 0x02 - 0x03

    /// <summary>
    /// Gets/sets device vendor ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public UInt16 VendorId {
        get { return (UInt16)((EepromBytes[0x03] << 8) | EepromBytes[0x02]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x02] = (byte)(value & 0xFF);
            EepromBytes[0x03] = (byte)(value >> 8);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion USB VID @ 0x02


    #region USB PID @ 0x04 - 0x05

    /// <summary>
    /// Gets/sets device product ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public UInt16 ProductId {
        get { return (UInt16)((EepromBytes[0x05] << 8) | EepromBytes[0x04]); }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x04] = (byte)(value & 0xFF);
            EepromBytes[0x05] = (byte)(value >> 8);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion USB PID @ 0x04 - 0x05


    #region Config Description Value @ 0x08

    /// <summary>
    /// Gets/sets remote wakeup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool RemoteWakeupEnabled {
        get { return (EepromBytes[0x08] & 0x20) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x08] = (byte)((EepromBytes[0x08] & ~0x20) | (value ? 0x20 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if device is self-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool SelfPowered {
        get { return (EepromBytes[0x08] & 0x40) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x08] = (byte)((EepromBytes[0x08] & ~0x40) | (value ? 0x40 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if device is bus-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool BusPowered {
        get { return !SelfPowered; }
        set { SelfPowered = !value; }
    }

    #endregion Config Description Value @ 0x08


    #region MAX Power @ 0x09

    /// <summary>
    /// Gets/sets device power requirement.
    /// All values are rounded to higher 2 mA.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Value must be between 1 and 500.</exception>
    public int MaxBusPower {
        get { return EepromBytes[0x09] * 2; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value is < 1 or > 500) { throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 1 and 500."); }
            var newValue = (value + 1) / 2;  // round up
            EepromBytes[0x09] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion MAX Power @ 0x09


    #region Device & Peripheral Control @ 0x0A - 0x0B

    /// <summary>
    /// Gets/sets if IO pins will be pulled down in USB suspend.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool PulldownPinsInSuspend {
        get { return (EepromBytes[0x0A] & 0x04) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0A] = (byte)((EepromBytes[0x0A] & ~0x04) | (value ? 0x04 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if serial number will be reported by device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool SerialNumberEnabled {
        get { return (EepromBytes[0x0A] & 0x08) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0A] = (byte)((EepromBytes[0x0A] & ~0x08) | (value ? 0x08 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if TXD is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool TxdInverted {
        get { return (EepromBytes[0x0B] & 0x01) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x01) | (value ? 0x01 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RXD is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool RxdInverted {
        get { return (EepromBytes[0x0B] & 0x02) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x02) | (value ? 0x02 : 0x00));
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
            return (EepromBytes[0x0B] & 0x04) != 0;
        }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x04) | (value ? 0x04 : 0x00));
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
            return (EepromBytes[0x0B] & 0x08) != 0;
        }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x08) | (value ? 0x08 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DTR is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DtrInverted {
        get { return (EepromBytes[0x0B] & 0x10) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x10) | (value ? 0x10 : 0x00));
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
            return (EepromBytes[0x0B] & 0x20) != 0;
        }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x20) | (value ? 0x20 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DCD is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool DcdInverted {
        get { return (EepromBytes[0x0B] & 0x40) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x40) | (value ? 0x40 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RI is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool RiInverted {
        get { return (EepromBytes[0x0B] & 0x80) != 0; }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0x0B] = (byte)((EepromBytes[0x0B] & ~0x80) | (value ? 0x80 : 0x00));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion Device & Peripheral Control @ 0x0A - 0x0B


    #region String Descriptors @ 0x0E - 0x13

    /// <summary>
    /// Sets string descriptors.
    /// </summary>
    /// <param name="manufacturer">Manufacturer name.</param>
    /// <param name="productDescription">Product description.</param>
    /// <param name="serialNumber">Serial number.</param>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Serial USB string can be up to 15 characters. -or- Not enough EEPROM space for USB string descriptors.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public void SetStringDescriptors(string manufacturer, string productDescription, string serialNumber) {
        if (manufacturer == null) { throw new ArgumentNullException(nameof(manufacturer), "Manufacturer cannot be null."); }
        if (productDescription == null) { throw new ArgumentNullException(nameof(productDescription), "Product decription cannot be null."); }
        if (serialNumber == null) { throw new ArgumentNullException(nameof(serialNumber), "Serial number cannot be null."); }

        if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
        if (EepromStrings.GetUnicodeCharacterCount(serialNumber) > 15 * 2) { throw new ArgumentOutOfRangeException(nameof(serialNumber), "Serial USB string can be up to 15 characters."); }
        if (!StringDescriptors.CheckUnicodeCharacterCount(manufacturer, productDescription, serialNumber)) { throw new ArgumentOutOfRangeException(nameof(manufacturer), "Not enough EEPROM space for USB string descriptors."); }

        StringDescriptors.SetEepromStrings(manufacturer, productDescription, serialNumber);
        SerialNumberEnabled = !string.IsNullOrEmpty(serialNumber);
        IsChecksumValid = true;  // fixup checksum
    }

    /// <summary>
    /// Gets/sets device manufacturer name.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Not enough EEPROM space for USB string descriptors.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public string Manufacturer {
        get { return StringDescriptors.Manufacturer; }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (!StringDescriptors.CheckUnicodeCharacterCount(value, ProductDescription, SerialNumber)) { throw new ArgumentOutOfRangeException(nameof(value), "Not enough EEPROM space for USB string descriptors."); }

            StringDescriptors.Manufacturer = value;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device product name.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Not enough EEPROM space for USB string descriptors.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public string ProductDescription {
        get { return StringDescriptors.ProductDescription; }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (!StringDescriptors.CheckUnicodeCharacterCount(Manufacturer, value, SerialNumber)) { throw new ArgumentOutOfRangeException(nameof(value), "Not enough EEPROM space for USB string descriptors."); }

            StringDescriptors.ProductDescription = value;
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device serial number.
    /// It will also enable/disable serial number reporting.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Serial USB string can be up to 15 characters. -or- Not enough EEPROM space for USB string descriptors.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public string SerialNumber {
        get { return StringDescriptors.SerialNumber; }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (EepromStrings.GetUnicodeCharacterCount(value) > 15 * 2) { throw new ArgumentOutOfRangeException(nameof(value), "Serial USB string can be up to 15 characters."); }
            if (!StringDescriptors.CheckUnicodeCharacterCount(Manufacturer, ProductDescription, value)) { throw new ArgumentOutOfRangeException(nameof(value), "Not enough EEPROM space for USB string descriptors."); }

            if (!string.IsNullOrEmpty(value)) {
                StringDescriptors.SerialNumber = value;
                SerialNumberEnabled = true;
            } else {
                StringDescriptors.SerialNumber = "";
                SerialNumberEnabled = false;
            }
            IsChecksumValid = true;  // fixup checksum
        }
    }

    #endregion String Descriptors @ 0x0E - 0x13


    /// <summary>
    /// Gets/sets if checksum is valid.
    /// If checksum is not valid, it can be made valid by setting the property to true.
    /// </summary>
    public abstract bool IsChecksumValid { get; set; }

}
