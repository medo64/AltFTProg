namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// FTDI device.
/// </summary>
internal class FtdiDevice {

    private FtdiDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, int eepromSize) {
        UsbDeviceHandle = usbDeviceHandle;
        UsbVendorId = usbVendorId;
        UsbProductId = usbProductId;

        DeviceType = type;
        EepromBytes = eepromBytes;
        EepromSize = eepromSize;
    }

    private readonly IntPtr UsbDeviceHandle;

    private readonly byte[] EepromBytes;


    #region USB Properties

    /// <summary>
    /// Gets USB device vendor ID.
    /// This is read from the device and not from the EEPROM.
    /// </summary>
    public int UsbVendorId { get; }

    /// <summary>
    /// Gets USB device product ID.
    /// This is read from the device and not from the EEPROM.
    /// </summary>
    public int UsbProductId { get; }

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

    #endregion USB Properties


    /// <summary>
    /// Gets EEPROM size.
    /// </summary>
    public int EepromSize { get; }

    /// <summary>
    /// Gets device type.
    /// </summary>
    public FtdiDeviceType DeviceType { get; }


    /// <summary>
    /// Gets/sets device manufacturer name.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public string? Manufacturer {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }

            GetEepromStrings(EepromBytes, out var manufacturer, out _, out _);
            return manufacturer;
        }
        set {
            if (DeviceType != FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

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
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public string? Product {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }

            GetEepromStrings(EepromBytes, out _, out var product, out _);
            return product;
        }
        set {
            if (DeviceType != FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

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
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Serial USB string can be up to 15 characters. -or- Combined length of manufacturer, product, and serial USB string can be up to 48 characters.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public string? Serial {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }

            GetEepromStrings(EepromBytes, out _, out _, out var serial);
            return serial;
        }
        set {
            if (DeviceType != FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

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
            return (UInt16)(EepromBytes[3] << 8 | EepromBytes[2]);
        }
        set {
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
            return (UInt16)(EepromBytes[5] << 8 | EepromBytes[4]);
        }
        set {
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[4] = (byte)(value & 0xFF);
            EepromBytes[5] = (byte)(value >> 8);
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets remote wakeup.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsRemoteWakeupEnabled {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[8] & 0x20) != 0;
        }
        set {
            if (DeviceType != FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[8] = (byte)((EepromBytes[8] & ~0x20) | (value.Value ? 0x20 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if device is self-powered.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsSelfPowered {
        get {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { return null; }
            return (EepromBytes[8] & 0x40) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[8] = (byte)((EepromBytes[8] & ~0x40) | (value.Value ? 0x40 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if device is bus-powered.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool? IsBusPowered {
        get { return !IsSelfPowered; }
        set { IsSelfPowered = !value; }
    }

    /// <summary>
    /// Gets/sets device power requirement.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public int? MaxPower {
        get {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { return null; }
            return EepromBytes[9] * 2;  // 2 mA unit
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (value is < 0 or > 500) { throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 500."); }

            var newValue = (value + 1) / 2;  // round up
            EepromBytes[9] = (byte)newValue;
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if IO pins will be pulled down in USB suspend.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsIOPulledDownDuringSuspend {
        get {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { return null; }
            return (EepromBytes[10] & 0x04) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[10] = (byte)((EepromBytes[10] & ~0x04) | (value.Value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if serial number will be reported by device.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsSerialNumberEnabled {
        get {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { throw new InvalidOperationException("Device not supported."); }
            return (EepromBytes[10] & 0x08) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R and not FtdiDeviceType.FTXSeries) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[10] = (byte)((EepromBytes[10] & ~0x08) | (value.Value ? 0x08 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets if TXD is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsTxdInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x01) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x01) | (value.Value ? 0x01 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RXD is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsRxdInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x02) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x02) | (value.Value ? 0x02 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RTS is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsRtsInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x04) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x04) | (value.Value ? 0x04 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if CTS is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsCtsInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x08) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x08) | (value.Value ? 0x08 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DTR is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsDtrInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x10) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x10) | (value.Value ? 0x10 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DSR is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsDsrInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x20) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x20) | (value.Value ? 0x20 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if DCD is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsDcdInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x40) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x40) | (value.Value ? 0x40 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if RI is inverted.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsRiInverted {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[11] & 0x80) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[11] = (byte)((EepromBytes[11] & ~0x80) | (value.Value ? 0x80 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets function for CBUS0.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public FtdiDevicePinFunction? CBus0Function {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (FtdiDevicePinFunction)(EepromBytes[20] & 0x0F);
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[20] = (byte)((EepromBytes[20] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS1.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public FtdiDevicePinFunction? CBus1Function {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (FtdiDevicePinFunction)(EepromBytes[20] >> 4);
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[20] = (byte)((EepromBytes[20] & 0x0F) | (newValue << 4));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS2.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public FtdiDevicePinFunction? CBus2Function {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (FtdiDevicePinFunction)(EepromBytes[21] & 0x0F);
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            var newValue = (int)value;
            if (newValue is < 0 or > 15) { throw new ArgumentOutOfRangeException(nameof(value), "Unsupported pin function value."); }
            EepromBytes[21] = (byte)((EepromBytes[21] & 0xF0) | newValue);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets function for CBUS3.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported pin function value (must be between 0 and 15).</exception>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public FtdiDevicePinFunction? CBus3Function {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (FtdiDevicePinFunction)(EepromBytes[21] >> 4);
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

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
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public FtdiDevicePinFunction? CBus4Function {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (FtdiDevicePinFunction)(EepromBytes[22] & 0x0F);
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

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
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public bool? IsHighCurrentIO {
        get {
            if (DeviceType != FtdiDeviceType.FT232R) { return null; }
            return (EepromBytes[0] & 0x04) != 0;
        }
        set {
            if (DeviceType is not FtdiDeviceType.FT232R) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }

            EepromBytes[0] = (byte)((EepromBytes[0] & ~0x04) | (value.Value ? 0x04 : 0));
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
            var eepromChecksum = (UInt16)(EepromBytes[EepromSize - 1] << 8 | EepromBytes[EepromSize - 2]);
            var checksum = GetChecksum(EepromBytes, EepromSize);
            return (eepromChecksum == checksum);
        }
        set {
            if (value == false) { throw new ArgumentException("Checksum validity cannot be set to false.", nameof(value)); }
            var checksum = GetChecksum(EepromBytes, EepromBytes.Length);
            EepromBytes[EepromSize - 2] = (byte)(checksum & 0xFF);
            EepromBytes[EepromSize - 1] = (byte)(checksum >> 8);
        }
    }


    #region EEPROM

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
        var eepromBytes = new byte[includeExtras ? EepromBytes.Length : EepromSize];
        Buffer.BlockCopy(EepromBytes, 0, eepromBytes, 0, eepromBytes.Length);
        return eepromBytes;
    }

    /// <summary>
    /// Write any changes to EEPROM.
    /// </summary>
    public void SaveChanges() {
        var ftdi = NativeMethods.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        try {
            ThrowIfError(ftdi, "ftdi_usb_open_dev", NativeMethods.ftdi_usb_open_dev(ftdi, UsbDeviceHandle));

            try {
                ThrowIfError(ftdi, "ftdi_write_eeprom", NativeMethods.ftdi_write_eeprom(ftdi, EepromBytes));
            } finally {
                NativeMethods.ftdi_usb_close(ftdi);
            }
        } finally {
            NativeMethods.ftdi_free(ftdi);
        }
    }

    #endregion EEPROM

    /// <summary>
    /// Returns collection of FTDI USB devices.
    /// </summary>
    public static IReadOnlyCollection<FtdiDevice> GetDevices() {
        return GetDevices(new KeyValuePair<int, int>[] {
            new(0x0403, 0x6001),
            new(0x0403, 0x6015),
        });
    }

    /// <summary>
    /// Returns collection of FTDI USB devices.
    /// </summary>
    /// <param name="vendorId">Vendor ID.</param>
    /// <param name="productId">Product ID.</param>
    /// <exception cref="ArgumentOutOfRangeException">Vendor ID must be between 0 and 65535. -or- Product ID must be between 0 and 65535.</exception>
    public static IReadOnlyCollection<FtdiDevice> GetDevices(int vendorId, int productId) {
        if (vendorId is < 0 or > 65535) { throw new ArgumentOutOfRangeException(nameof(vendorId), "Vendor ID must be between 0 and 65535."); }
        if (productId is < 0 or > 65535) { throw new ArgumentOutOfRangeException(nameof(productId), "Product ID must be between 0 and 65535."); }

        return GetDevices(new KeyValuePair<int, int>[] { new(vendorId, productId) });
    }


    #region Helpers

    private static IReadOnlyCollection<FtdiDevice> GetDevices(IEnumerable<KeyValuePair<int, int>> vidPids) {
        var ftdi = NativeMethods.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        var devices = new List<FtdiDevice>();

        try {
            foreach (var vidPid in vidPids) {
                var vendorId = vidPid.Key;
                var productId = vidPid.Value;

                var deviceList = IntPtr.Zero;

                try {
                    var findRes = NativeMethods.ftdi_usb_find_all(ftdi, ref deviceList, vendorId, productId);
                    ThrowIfError(ftdi, "ftdi_usb_find_all", findRes);

                    var currDevice = deviceList;
                    while (currDevice != IntPtr.Zero) {
                        var deviceStruct = (NativeMethods.ftdi_device_list)Marshal.PtrToStructure(currDevice, typeof(NativeMethods.ftdi_device_list))!;
                        var usbDeviceHandle = deviceStruct.dev;

                        var rawEepromBytes = GetRawEepromBytes(usbDeviceHandle);
                        var i = rawEepromBytes.Length - 1;
                        for (; i >= 0; i--) {
                            if (rawEepromBytes[i] != 0xFF) { break; }
                        }
                        var nonEmptyBlockCount = i / 128 + 1;

                        var eepromExtraSize = nonEmptyBlockCount * 128;
                        var eepromBytes = new byte[eepromExtraSize];
                        Buffer.BlockCopy(rawEepromBytes, 0, eepromBytes, 0, eepromBytes.Length);

                        var type = (FtdiDeviceType)((rawEepromBytes[7] << 8) | rawEepromBytes[6]);

                        var eepromSize = eepromExtraSize;
                        if (type == FtdiDeviceType.FT232R) { eepromSize = 128; }  // for some reason FT232R reports 256 bytes, but only 128 are user data

                        var device = new FtdiDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes, eepromSize);
                        devices.Add(device);

                        currDevice = deviceStruct.next;
                    }
                } finally {
                    if (deviceList != IntPtr.Zero) {
                        NativeMethods.ftdi_list_free(ref deviceList);
                    }
                }
            }
        } finally {
            NativeMethods.ftdi_free(ftdi);
        }

        return (devices.AsReadOnly());
    }

    [StackTraceHidden()]
    private static void ThrowIfError(IntPtr ftdi, string errorSource, int errorCode) {
        if (errorCode < 0) {
            var errorPointer = NativeMethods.ftdi_get_error_string(ftdi);
            if (errorPointer == IntPtr.Zero) {
                throw new InvalidOperationException(errorSource + " failed with error code " + errorCode.ToString() + ".");
            } else {
                var errorText = Marshal.PtrToStringUTF8(errorPointer);
                throw new InvalidOperationException(errorSource + " failed with error code " + errorCode.ToString() + ": " + errorText);
            }
        }
    }

    private static void GetUsbStrings(IntPtr usbDeviceHandle, out string manufacturer, out string description, out string serial) {
        var ftdi = NativeMethods.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        try {
            var sbManufacturer = new StringBuilder(256);
            var sbDescription = new StringBuilder(256);
            var sbSerial = new StringBuilder(256);

            var errorCode = NativeMethods.ftdi_usb_get_strings(ftdi, usbDeviceHandle,
                sbManufacturer, sbManufacturer.Capacity,
                sbDescription, sbDescription.Capacity,
                sbSerial, sbSerial.Capacity);
            ThrowIfError(ftdi, "ftdi_usb_get_strings", errorCode);

            manufacturer = sbManufacturer.ToString();
            description = sbDescription.ToString();
            serial = sbSerial.ToString();
        } finally {
            NativeMethods.ftdi_free(ftdi);
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

    /// <summary>
    /// Gets all EEPROM bytes without any processing.
    /// </summary>
    private static byte[] GetRawEepromBytes(IntPtr usbDeviceHandle) {
        var ftdi = NativeMethods.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        try {
            ThrowIfError(ftdi, "ftdi_usb_open_dev", NativeMethods.ftdi_usb_open_dev(ftdi, usbDeviceHandle));

            try {
                var rawEepromBytes = new byte[4096];
                var len = NativeMethods.ftdi_read_eeprom_getsize(ftdi, rawEepromBytes, rawEepromBytes.Length);
                ThrowIfError(ftdi, "ftdi_read_eeprom", len);

                var eepromBytes = new byte[len];
                Buffer.BlockCopy(rawEepromBytes, 0, eepromBytes, 0, eepromBytes.Length);
                return eepromBytes;
            } finally {
                NativeMethods.ftdi_usb_close(ftdi);
            }
        } finally {
            NativeMethods.ftdi_free(ftdi);
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

    #endregion Helpers


    private static class NativeMethods {

        /** FTDI chip type */
        public enum ftdi_chip_type {
            TYPE_AM = 0,
            TYPE_BM = 1,
            TYPE_2232C = 2,
            TYPE_R = 3,
            TYPE_2232H = 4,
            TYPE_4232H = 5,
            TYPE_232H = 6,
            TYPE_230X = 7,
        };

        /** Automatic loading / unloading of kernel modules */
        public enum ftdi_module_detach_mode {
            AUTO_DETACH_SIO_MODULE = 0,
            DONT_DETACH_SIO_MODULE = 1,
            AUTO_DETACH_REATACH_SIO_MODULE = 2,
        };


        /** Main context structure for all libftdi functions */
        [StructLayout(LayoutKind.Sequential)]
        public struct ftdi_context {
            /* USB specific */
            /** libusb's usb_dev_handle */
            public IntPtr usb_dev;
            /** usb read timeout */
            public int usb_read_timeout;
            /** usb write timeout */
            public int usb_write_timeout;

            // FTDI specific
            /* FTDI chip type */
            public ftdi_chip_type type;
            /** baudrate */
            public int baudrate;
            /** bitbang mode state */
            public byte bitbang_enabled;
            /** pointer to read buffer for ftdi_read_data */
            public IntPtr readbuffer;
            /** read buffer offset */
            public uint readbuffer_offset;
            /** number of remaining data in internal read buffer */
            public uint readbuffer_remaining;
            /** read buffer chunk size */
            public uint readbuffer_chunksize;
            /** write buffer chunk size */
            public uint writebuffer_chunksize;
            /** maximum packet size. Needed for filtering modem status bytes every n packets. */
            public uint max_packet_size;

            /* FTDI FT2232C requirecments */
            /** FT2232C interface number: 0 or 1 */
            public int @interface;   /* 0 or 1 */
            /** FT2232C index number: 1 or 2 */
            public int index;       /* 1 or 2 */
            /* Endpoints */
            /** FT2232C end points: 1 or 2 */
            public int in_ep;
            public int out_ep;      /* 1 or 2 */

            /** Bitbang mode. 1: (default) Normal bitbang mode, 2: FT2232C SPI bitbang mode */
            public byte bitbang_mode;

            /** EEPROM size. Default is 128 bytes for 232BM and 245BM chips */
            public int eeprom_size;

            /** String representation of last error */
            public IntPtr error_str;           /* const char * */

            /** Buffer needed for async communication */
            public IntPtr async_usb_buffer;
            /** Number of URB-structures we can buffer */
            public uint async_usb_buffe_rsize;

            /** Defines behavior in case a kernel module is already attached to the device */
            public ftdi_module_detach_mode module_detach_mode;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct ftdi_device_list {
            /** pointer to next entry */
            public IntPtr next;
            /** pointer to libusb's usb_device */
            public IntPtr dev;
        };


        [DllImport("libftdi.so")]
        public static extern void ftdi_free(
            IntPtr ftdi
        );

        [DllImport("libftdi.so")]
        public static extern IntPtr ftdi_new(
        );

        [DllImport("libftdi.so")]
        public static extern void ftdi_list_free(
            ref IntPtr devlist
        );

        [DllImport("libftdi.so")]
        public static extern int ftdi_read_eeprom_getsize(
            IntPtr ftdi,
            [Out] byte[] eeprom,
            int maxsize
        );

        [DllImport("libftdi.so")]
        public static extern int ftdi_write_eeprom(
            IntPtr ftdi,
            byte[] eeprom
        );

        [DllImport("libftdi.so")]
        public static extern int ftdi_usb_find_all(
            IntPtr ftdi,
            ref IntPtr devlist,
            int vendor,
            int product
        );

        [DllImport("libftdi.so")]
        public static extern int ftdi_usb_close(
            IntPtr ftdi
        );

        [DllImport("libftdi.so")]
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

        [DllImport("libftdi.so")]
        public static extern int ftdi_usb_open_dev(
            IntPtr ftdi,
            IntPtr dev
        );

        [DllImport("libftdi.so")]
        public static extern IntPtr ftdi_get_error_string(
            IntPtr ftdi
        );
    }
}



/// <summary>
/// FTDI chip type.
/// </summary>
public enum FtdiDeviceType {
    /// <summary>
    /// FT232/245AM (0403:6001).
    /// </summary>
    FT232A = 512,

    /// <summary>
    /// FT232/245BM (0403:6001).
    /// </summary>
    FT232B = 1024,

    /// <summary>
    /// FT2232D (0403:6010).
    /// </summary>
    FT2232D = 1280,

    /// <summary>
    /// FT232R/FT245R (0403:6001).
    /// </summary>
    FT232R = 1536,

    /// <summary>
    /// FT2232H (0403:6010).
    /// </summary>
    FT2232H = 1792,

    /// <summary>
    /// FT232H (0403:6014).
    /// </summary>
    FT232H = 2304,

    /// <summary>
    /// FT X Series (0403:6015).
    /// </summary>
    FTXSeries = 4096,
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
