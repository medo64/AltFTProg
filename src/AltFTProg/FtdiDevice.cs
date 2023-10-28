namespace AltFTProg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// FTDI device.
/// </summary>
internal class FtdiDevice {

    private FtdiDevice(IntPtr usbDeviceHandle) {
        UsbDeviceHandle = usbDeviceHandle;
    }

    private readonly IntPtr UsbDeviceHandle;
    private byte[]? EepromBytes;


    private string? _usbManufacturer;
    /// <summary>
    /// Gets USB device manufacturer name.
    /// This is read from the device and not from the EEPROM.
    /// </summary>
    public string UsbManufacturer {
        get {
            if (_usbManufacturer == null) { GetUsbStrings(UsbDeviceHandle, out _usbManufacturer, out _usbProduct, out _usbSerial); }
            return _usbManufacturer;
        }
    }

    private string? _usbProduct;
    /// <summary>
    /// Gets USB device product.
    /// This is read from the device and not from the EEPROM.
    /// </summary>
    public string UsbProduct {
        get {
            if (_usbProduct == null) { GetUsbStrings(UsbDeviceHandle, out _usbManufacturer, out _usbProduct, out _usbSerial); }
            return _usbProduct;
        }
    }

    private string? _usbSerial;
    /// <summary>
    /// Gets USB device serial number.
    /// This is read from the device and not from the EEPROM.
    /// </summary>
    public string UsbSerial {
        get {
            if (_usbSerial == null) { GetUsbStrings(UsbDeviceHandle, out _usbManufacturer, out _usbProduct, out _usbSerial); }
            return _usbSerial;
        }
    }

    /// <summary>
    /// Gets internal serial number of FTDI device, if one is found.
    /// </summary>
    public string? InnerSerial {
        get {
            var bytes = GetEepromBytes(includeExtras: true);
            if (bytes.Length < 256) { return null; }
            return Encoding.ASCII.GetString(bytes, 0x98, 8);
        }
    }


    /// <summary>
    /// Gets/sets device manufacturer name.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    public string Manufacturer {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            GetEepromStrings(EepromBytes, out var manufacturer, out _, out _);
            return manufacturer;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            GetEepromStrings(EepromBytes, out _, out var product, out var serial);
            try {
                SetEepromStrings(EepromBytes, value, product, serial);
                IsChecksumValid = true;  // fixup checksum
            } catch (InvalidOperationException ex) {
                throw new ArgumentOutOfRangeException("Combined length of manufacturer, product, and serial USB string can be up to 48 characters.", ex);
            }
        }
    }

    /// <summary>
    /// Gets/sets device product name.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    public string Product {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            GetEepromStrings(EepromBytes, out _, out var product, out _);
            return product;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            GetEepromStrings(EepromBytes, out var manufacturer, out _, out var serial);
            try {
                SetEepromStrings(EepromBytes, manufacturer, value, serial);
                IsChecksumValid = true;  // fixup checksum
            } catch (InvalidOperationException ex) {
                throw new ArgumentOutOfRangeException("Combined length of manufacturer, product, and serial USB string can be up to 48 characters.", ex);
            }
        }
    }

    /// <summary>
    /// Gets/sets device serial number.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Serial USB string can be up to 15 characters. -or- Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    public string Serial {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            GetEepromStrings(EepromBytes, out _, out _, out var serial);
            return serial;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (Encoding.Unicode.GetBytes(value).Length > 30) { throw new ArgumentOutOfRangeException(nameof(value), "Serial USB string can be up to 15 characters."); }
            GetEepromStrings(EepromBytes, out var manufacturer, out var product, out _);
            try {
                SetEepromStrings(EepromBytes, manufacturer, product, value);
                IsChecksumValid = true;  // fixup checksum
            } catch (InvalidOperationException ex) {
                throw new ArgumentOutOfRangeException("Combined length of manufacturer, product, and serial USB string can be up to 48 characters.", ex);
            }
        }
    }


    /// <summary>
    /// Gets/sets device vendor ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public UInt16 VendorId {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (UInt16)(EepromBytes[3] << 8 | EepromBytes[2]);
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[2] = (byte)(value & 0xFF);
            EepromBytes[3] = (byte)(value >> 8);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device product ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public UInt16 ProductId {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (UInt16)(EepromBytes[5] << 8 | EepromBytes[4]);
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[4] = (byte)(value & 0xFF);
            EepromBytes[5] = (byte)(value >> 8);
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets device chip type.
    /// </summary>
    public FtdiDeviceChipType ChipType {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (FtdiDeviceChipType)EepromBytes[7];
        }
    }


    /// <summary>
    /// Gets/sets USB remote wakeup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsRemoteWakeupEnabled {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[8] & 0x20) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[8] = (byte)((EepromBytes[8] & ~0x20) | (value ? 0x20 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if device is self-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsSelfPowered {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[8] & 0x40) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[8] = (byte)((EepromBytes[8] & ~0x40) | (value ? 0x40 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if device is bus-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsBusPowered {
        get { return !IsSelfPowered; }
        set { IsSelfPowered = !value; }
    }

    /// <summary>
    /// Gets/sets device power requirement.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Value must be between 0 and 500.</exception>
    public int MaxPower {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return EepromBytes[9] * 2;  // 2 mA unit
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
    public bool IsIOPulledDownDuringSuspend {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[10] & 0x04) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[10] = (byte)((EepromBytes[10] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if serial number will be reported by device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsSerialNumberEnabled {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[10] & 0x08) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x01) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x02) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x02) | (value ? 0x02 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RTS is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsRtsInverted {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x04) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if CTS is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsCtsInverted {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x08) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x08) | (value ? 0x08 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DTR is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsDtrInverted {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x10) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x10) | (value ? 0x10 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DSR is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsDsrInverted {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x20) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x20) | (value ? 0x20 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DCD is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsDcdInverted {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x40) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x40) | (value ? 0x40 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RI is inverted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsRiInverted {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[11] & 0x80) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x80) | (value ? 0x80 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    public FtdiDevicePinFunction CBus0Function {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (FtdiDevicePinFunction)(EepromBytes[20] & 0x0F);
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    public FtdiDevicePinFunction CBus1Function {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (FtdiDevicePinFunction)(EepromBytes[20] >> 4);
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    public FtdiDevicePinFunction CBus2Function {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (FtdiDevicePinFunction)(EepromBytes[21] & 0x0F);
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    public FtdiDevicePinFunction CBus3Function {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (FtdiDevicePinFunction)(EepromBytes[21] >> 4);
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    public FtdiDevicePinFunction CBus4Function {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (FtdiDevicePinFunction)(EepromBytes[22] & 0x0F);
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
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
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsHighCurrentIO {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[0] & 0x04) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x04) | (value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if checksum is valid.
    /// If checksum is not valid, it can be made valid by setting the property to true.
    /// </summary>
    /// <exception cref="ArgumentException">Checksum validity cannot be set to false.</exception>
    public bool IsChecksumValid {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            var eepromChecksum = (UInt16)(EepromBytes[EepromBytes.Length - 1] << 8 | EepromBytes[EepromBytes.Length - 2]);
            var checksum = GetChecksum(EepromBytes, EepromBytes.Length);
            return (eepromChecksum == checksum);
        }
        set {
            if (value == false) { throw new ArgumentException("Checksum validity cannot be set to false.", nameof(value)); }
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            var checksum = GetChecksum(EepromBytes, EepromBytes.Length);
            EepromBytes[EepromBytes.Length - 2] = (byte)(checksum & 0xFF);
            EepromBytes[EepromBytes.Length - 1] = (byte)(checksum >> 8);
        }
    }


    /// <summary>
    /// Returns all EEPROM bytes for the USB device.
    /// </summary>
    public byte[] GetEepromBytes() {
        return GetEepromBytes(includeExtras: false);
    }

    /// <summary>
    /// Returns all EEPROM bytes for the USB device.
    /// </summary>
    /// <param name="includeExtras">Include extra EEPROM data.</param>
    public byte[] GetEepromBytes(bool includeExtras) {
        var ftdi = Marshal.AllocHGlobal(4096);  // more than enough bytes for ftdi_context struct (112 bytes)
        try {
            var initRes = NativeMethods.ftdi_init(ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            try {
                NativeMethods.ftdi_usb_open_dev(ftdi, UsbDeviceHandle);

                var eeprom = new byte[4096];
                var eepromFullLen = NativeMethods.ftdi_read_eeprom_getsize(ftdi, eeprom, eeprom.Length);
                if (eepromFullLen < 0) { throw new InvalidOperationException("ftdi_read_eeprom failed with error code " + eepromFullLen.ToString()); }

                var eepromLen = 128;  // ftdi_read_eeprom_getsize returns more data than the actual EEPROM size; assume 128 bytes

                var eepromBytes = new byte[includeExtras ? eepromFullLen : eepromLen];  // function returns number of bytes read
                Buffer.BlockCopy(eeprom, 0, eepromBytes, 0, eepromBytes.Length);

                return eepromBytes;
            } finally {
                NativeMethods.ftdi_usb_close(ftdi);
            }
        } finally {
            NativeMethods.ftdi_deinit(ftdi);
        }
    }


    /// <summary>
    /// Returns EEPROM bytes preparred to be written.
    /// </summary>
    public byte[] GetNewEepromBytes() {
        if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }

        var eepromBytes = new byte[EepromBytes.Length];
        Buffer.BlockCopy(EepromBytes, 0, eepromBytes, 0, eepromBytes.Length);
        return eepromBytes;
    }

    /// <summary>
    /// Write any changes to EEPROM.
    /// </summary>
    public void SaveChanges() {
        var eepromBytes = GetNewEepromBytes();

        var ftdi = Marshal.AllocHGlobal(4096);  // more than enough bytes for ftdi_context struct (112 bytes)
        try {
            var initRes = NativeMethods.ftdi_init(ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            try {
                NativeMethods.ftdi_usb_open_dev(ftdi, UsbDeviceHandle);

                var result = NativeMethods.ftdi_write_eeprom(ftdi, eepromBytes);
                if (result < 0) { throw new InvalidOperationException("ftdi_write_eeprom failed with error code " + result.ToString()); }
            } finally {
                NativeMethods.ftdi_usb_close(ftdi);
            }
        } finally {
            NativeMethods.ftdi_deinit(ftdi);
        }
    }


    private static void GetUsbStrings(IntPtr usbDeviceHandle, out string manufacturer, out string description, out string serial) {
        var ftdi = Marshal.AllocHGlobal(4096);  // more than enough bytes for ftdi_context struct (112 bytes)
        try {
            var initRes = NativeMethods.ftdi_init(ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            var sbManufacturer = new StringBuilder(256);
            var sbDescription = new StringBuilder(256);
            var sbSerial = new StringBuilder(256);
            NativeMethods.ftdi_usb_get_strings(ftdi, usbDeviceHandle,
                sbManufacturer, sbManufacturer.Capacity,
                sbDescription, sbDescription.Capacity,
                sbSerial, sbSerial.Capacity);

            manufacturer = sbManufacturer.ToString();
            description = sbDescription.ToString();
            serial = sbSerial.ToString();
        } finally {
            NativeMethods.ftdi_deinit(ftdi);
        }
    }

    private static void GetEepromStrings(byte[] eepromBytes, out string manufacturer, out string product, out string serial) {
        if ((eepromBytes[0x0E] & 0x80) == 0) { throw new InvalidOperationException("Manufacturer EEPROM field not detected."); }
        if ((eepromBytes[0x10] & 0x80) == 0) { throw new InvalidOperationException("Product EEPROM field not detected."); }
        if ((eepromBytes[0x12] & 0x80) == 0) { throw new InvalidOperationException("Serial EEPROM field not detected."); }

        var offsetManufacturer = eepromBytes[0x0E] & 0x7F;
        var offsetProduct = eepromBytes[0x10] & 0x7F;
        var offsetSerial = eepromBytes[0x12] & 0x7F;

        var len1Manufacturer = eepromBytes[0x0F];
        var len1Product = eepromBytes[0x11];
        var len1Serial = eepromBytes[0x13];

        var len2Manufacturer = eepromBytes[offsetManufacturer];
        var len2Product = eepromBytes[offsetProduct];
        var len2Serial = eepromBytes[offsetSerial];

        if (len1Manufacturer != len2Manufacturer) { throw new InvalidOperationException("Manufacturer EEPROM field length mismatch."); }
        if (len1Product != len2Product) { throw new InvalidOperationException("Product EEPROM field length mismatch."); }
        if (len1Serial != len2Serial) { throw new InvalidOperationException("Serial EEPROM field length mismatch."); }

        if (len1Manufacturer < 2) { throw new InvalidOperationException("Manufacturer EEPROM field length too small."); }
        if (len1Product < 2) { throw new InvalidOperationException("Product EEPROM field length too small."); }
        if (len1Serial < 2) { throw new InvalidOperationException("Serial EEPROM field length too small."); }

        if (offsetManufacturer + len1Manufacturer >= 128) { throw new InvalidOperationException("Manufacturer EEPROM field outside of bound."); }
        if (offsetProduct + len1Product >= 128) { throw new InvalidOperationException("Product EEPROM field outside of bound."); }
        if (offsetSerial + len1Serial >= 128) { throw new InvalidOperationException("Serial EEPROM field outside of bound."); }

        if (eepromBytes[offsetManufacturer + 1] != 0x03) { throw new InvalidOperationException("Manufacturer EEPROM field type mismatch."); }
        if (eepromBytes[offsetProduct + 1] != 0x03) { throw new InvalidOperationException("Product EEPROM field type mismatch."); }
        if (eepromBytes[offsetSerial + 1] != 0x03) { throw new InvalidOperationException("Serial EEPROM field type mismatch."); }

        manufacturer = GetEepromString(eepromBytes, offsetManufacturer, len1Manufacturer);
        product = GetEepromString(eepromBytes, offsetProduct, len1Product);
        serial = GetEepromString(eepromBytes, offsetSerial, len1Serial);
    }

    private static string GetEepromString(byte[] eepromBytes, int offset, int length) {
        var stringBytes = new byte[length - 2];
        Buffer.BlockCopy(eepromBytes, offset + 2, stringBytes, 0, stringBytes.Length);
        return Encoding.Unicode.GetString(stringBytes);
    }

    private static void SetEepromStrings(byte[] eepromBytes, string manufacturer, string product, string serial) {
        var manufacturerBytes = Encoding.Unicode.GetBytes(manufacturer);
        var productBytes = Encoding.Unicode.GetBytes(product);
        var serialBytes = Encoding.Unicode.GetBytes(serial);

        var lenManufacturer = manufacturerBytes.Length + 2;
        var lenProduct = productBytes.Length + 2;
        var lenSerial = serialBytes.Length + 2;

        var offsetFirst = eepromBytes[0x0E] & 0x7F;
        var offsetManufacturer = offsetFirst;
        var offsetProduct = offsetManufacturer + lenManufacturer;
        var offsetSerial = offsetProduct + lenProduct;
        var offsetLast = offsetSerial + lenSerial;
        if (offsetLast > 126) { throw new InvalidOperationException("Not enough memory to write USB strings."); }

        eepromBytes[0x0E] = (byte)(0x80 | offsetManufacturer);
        eepromBytes[0x0F] = (byte)(lenManufacturer);
        eepromBytes[0x10] = (byte)(0x80 | offsetProduct);
        eepromBytes[0x11] = (byte)(lenProduct);
        eepromBytes[0x12] = (byte)(0x80 | offsetSerial);
        eepromBytes[0x13] = (byte)(lenSerial);

        eepromBytes[offsetManufacturer + 0] = (byte)lenManufacturer;
        eepromBytes[offsetManufacturer + 1] = 0x03;
        for (var i = 0; i < manufacturerBytes.Length; i++) {
            eepromBytes[offsetManufacturer + 2 + i] = manufacturerBytes[i];
        }

        eepromBytes[offsetProduct + 0] = (byte)lenProduct;
        eepromBytes[offsetProduct + 1] = 0x03;
        for (var i = 0; i < productBytes.Length; i++) {
            eepromBytes[offsetProduct + 2 + i] = productBytes[i];
        }

        eepromBytes[offsetSerial + 0] = (byte)lenSerial;
        eepromBytes[offsetSerial + 1] = 0x03;
        for (var i = 0; i < serialBytes.Length; i++) {
            eepromBytes[offsetSerial + 2 + i] = serialBytes[i];
        }

        for (var i = offsetLast; i < 126; i++) {
            eepromBytes[i] = 0;
        }
    }

    private static ushort GetChecksum(byte[] eeprom, int eepromSize) {
        UInt16 crc = 0xAAAA;
        for (var i = 0; i < eepromSize - 2; i += 2) {
            crc ^= (UInt16)(eeprom[i] | (eeprom[i + 1] << 8));
            crc = (UInt16)((crc << 1) | (crc >> 15));
        }
        return crc;
    }


    /// <summary>
    /// Returns collection of FTDI USB devices.
    /// </summary>
    public static IReadOnlyCollection<FtdiDevice> GetDevices() {
        var ftdi = Marshal.AllocHGlobal(4096);  // more than enough bytes for ftdi_context struct (112 bytes)
        var deviceList = IntPtr.Zero;

        try {
            var initRes = NativeMethods.ftdi_init(ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            var findRes = NativeMethods.ftdi_usb_find_all(ftdi, ref deviceList, 0x0403, 0x6001);
            if (findRes < 0) { throw new InvalidOperationException("ftdi_usb_find_all with error code " + initRes.ToString()); }

            var devices = new List<FtdiDevice>();

            var currDevice = deviceList;
            while (currDevice != IntPtr.Zero) {
                var deviceStruct = (NativeMethods.ftdi_device_list)Marshal.PtrToStructure(currDevice, typeof(NativeMethods.ftdi_device_list))!;

                var device = new FtdiDevice(deviceStruct.dev);
                var eepromBytes = device.GetEepromBytes();
                if (eepromBytes.Length < 128) { continue; }  // skip devices with EEPROM smaller than 128 bytes (no such device, but just in case)
                if (eepromBytes[7] != 0x06) { continue; }  // skip non-FT232H devices; this will load EEPROM data - ok for now

                devices.Add(device);
                currDevice = deviceStruct.next;
            }

            return (devices.AsReadOnly());
        } finally {
            NativeMethods.ftdi_deinit(ftdi);
            if (deviceList != IntPtr.Zero) {
                NativeMethods.ftdi_list_free(ref deviceList);
            }
        }
    }


    private static class NativeMethods {

        [StructLayout(LayoutKind.Sequential)]
        public struct ftdi_device_list {
            /** pointer to next entry */
            public IntPtr next;
            /** pointer to libusb's usb_device */
            public IntPtr dev;
        };


        [DllImport("libftdi")]
        public static extern void ftdi_deinit(
            IntPtr ftdi
        );

        [DllImport("libftdi")]
        public static extern int ftdi_init(
            IntPtr ftdi
        );

        [DllImport("libftdi")]
        public static extern void ftdi_list_free(
            ref IntPtr devlist
        );

        [DllImport("libftdi")]
        public static extern int ftdi_read_eeprom(
            IntPtr ftdi,
            [Out] byte[] eeprom
        );

        [DllImport("libftdi")]
        public static extern int ftdi_read_eeprom_getsize(
            IntPtr ftdi,
            [Out] byte[] eeprom,
            int maxsize
        );

        [DllImport("libftdi")]
        public static extern int ftdi_write_eeprom(
            IntPtr ftdi,
            byte[] eeprom
        );

        [DllImport("libftdi")]
        public static extern int ftdi_usb_find_all(
            IntPtr ftdi,
            ref IntPtr devlist,
            int vendor,
            int product
        );

        [DllImport("libftdi")]
        public static extern int ftdi_usb_close(
            IntPtr ftdi
        );

        [DllImport("libftdi")]
        public static extern int ftdi_usb_get_strings(
            IntPtr ftdi,
            IntPtr dev,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder manufacturer,
            int mnf_len,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder description,
            int desc_len,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder serial,
            int serial_len
        );

        [DllImport("libftdi")]
        public static extern int ftdi_usb_open_dev(
            IntPtr ftdi,
            IntPtr dev
        );

    }
}



/// <summary>
/// FTDI chip type.
/// </summary>
public enum FtdiDeviceChipType {
    /// <summary>
    /// FTDI AM chip type.
    /// </summary>
    FtdiAM = 0,

    /// <summary>
    /// FTDI BM chip type.
    /// </summary>
    FtdiBM = 1,

    /// <summary>
    /// FTDI 2232C chip type.
    /// </summary>
    Ftdi2232C = 2,

    /// <summary>
    /// FTDI R chip type.
    /// </summary>
    FtdiR = 3,

    /// <summary>
    /// FTDI 2232H chip type.
    /// </summary>
    Ftdi2232H = 4,

    /// <summary>
    /// FTDI 4232H chip type.
    /// </summary>
    Ftdi4232H = 5,

    /// <summary>
    /// FTDI 232H chip type.
    /// </summary>
    Ftdi232H = 6,
};



/// <summary>
/// FTDI pin function.
/// </summary>
public enum FtdiDevicePinFunction {
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
