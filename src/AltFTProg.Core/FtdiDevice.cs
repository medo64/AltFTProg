namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AltFTProg.NativeMethods;

/// <summary>
/// FTDI device.
/// </summary>
public abstract class FtdiDevice {

    internal protected FtdiDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, int eepromSize) {
        UsbDeviceHandle = usbDeviceHandle;
        UsbVendorId = usbVendorId;
        UsbProductId = usbProductId;

        DeviceType = type;
        EepromBytes = eepromBytes;
        EepromSize = eepromSize;
    }

    private readonly IntPtr UsbDeviceHandle;

    protected readonly byte[] EepromBytes;


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
    /// Gets/sets device vendor ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public UInt16 VendorId {
        get {
            return (UInt16)(EepromBytes[3] << 8 | EepromBytes[2]);
        }
        set {
            if (DeviceType is not (FtdiDeviceType.FT232R or FtdiDeviceType.FTXSeries)) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[2] = (byte)(value & 0xFF);
            EepromBytes[3] = (byte)(value >> 8);
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets device product ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported. -or- Current checksum is invalid.</exception>
    public UInt16 ProductId {
        get {
            return (UInt16)(EepromBytes[5] << 8 | EepromBytes[4]);
        }
        set {
            if (DeviceType is not (FtdiDeviceType.FT232R or FtdiDeviceType.FTXSeries)) { throw new InvalidOperationException("Device not supported."); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[4] = (byte)(value & 0xFF);
            EepromBytes[5] = (byte)(value >> 8);
            IsChecksumValid = true;  // fixup checksum
        }
    }


    /// <summary>
    /// Gets/sets device manufacturer name.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual string Manufacturer {
        get {
            Helpers.GetEepromStrings(EepromBytes, EepromSize, out var manufacturer, out _, out _);
            return manufacturer;
        }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets device product name.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual string Product {
        get {
            Helpers.GetEepromStrings(EepromBytes, EepromSize, out _, out var product, out _);
            return product;
        }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets device serial number.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual string Serial {
        get {
            Helpers.GetEepromStrings(EepromBytes, EepromSize, out _, out _, out var serial);
            return serial;
        }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets remote wakeup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public  virtual bool IsRemoteWakeupEnabled {
        get { return (EepromBytes[8] & 0x20) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets if device is self-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual bool IsSelfPowered {
        get { return (EepromBytes[8] & 0x40) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets if device is bus-powered.
    /// </summary>
    public bool IsBusPowered {
        get { return !IsSelfPowered; }
        set { IsSelfPowered = !value; }
    }

    /// <summary>
    /// Gets/sets device power requirement.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual int MaxPower {
        get { return EepromBytes[9] * 2; }  // 2 mA unit
        set { throw new InvalidOperationException("Device not supported."); }
    }


    /// <summary>
    /// Gets/sets if IO pins will be pulled down in USB suspend.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual bool IsIOPulledDownDuringSuspend {
        get { return (EepromBytes[10] & 0x04) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets if serial number will be reported by device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual bool IsSerialNumberEnabled {
        get { return (EepromBytes[10] & 0x08) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
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
            var checksum = Helpers.GetChecksum(EepromBytes, EepromSize, DeviceType);
            return (eepromChecksum == checksum);
        }
        set {
            if (value == false) { throw new ArgumentException("Checksum validity cannot be set to false.", nameof(value)); }
            var checksum = Helpers.GetChecksum(EepromBytes, EepromSize, DeviceType);
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
        var ftdi = LibFtdi.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        try {
            ThrowIfError(ftdi, "ftdi_usb_open_dev", LibFtdi.ftdi_usb_open_dev(ftdi, UsbDeviceHandle));

            try {
                ThrowIfError(ftdi, "ftdi_write_eeprom", LibFtdi.ftdi_write_eeprom(ftdi, EepromBytes));
            } finally {
                LibFtdi.ftdi_usb_close(ftdi);
            }
        } finally {
            LibFtdi.ftdi_free(ftdi);
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

    private static IReadOnlyCollection<FtdiDevice> GetDevices(IEnumerable<KeyValuePair<int, int>> vidPids) {
        var ftdi = LibFtdi.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        var devices = new List<FtdiDevice>();

        try {
            foreach (var vidPid in vidPids) {
                var vendorId = vidPid.Key;
                var productId = vidPid.Value;

                var deviceList = IntPtr.Zero;

                try {
                    var findRes = LibFtdi.ftdi_usb_find_all(ftdi, ref deviceList, vendorId, productId);
                    ThrowIfError(ftdi, "ftdi_usb_find_all", findRes);

                    var currDevice = deviceList;
                    while (currDevice != IntPtr.Zero) {
                        var deviceStruct = (LibFtdi.ftdi_device_list)Marshal.PtrToStructure(currDevice, typeof(LibFtdi.ftdi_device_list))!;
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

                        FtdiDevice device;
                        switch (type) {
                            case FtdiDeviceType.FT232R: device = new Ftdi232RDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes, eepromSize); break;
                            case FtdiDeviceType.FTXSeries: device = new FtdiXSeriesDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes, eepromSize); break;
                            default:
                                if ((type == 0) && (eepromSize == 256) && (rawEepromBytes.Length == 256)) {
                                    device = new Ftdi232RDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes, 128);
                                } else {
                                    device = new FtdiUnknownDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes, eepromSize);
                                }
                                break;
                        };
                        devices.Add(device);

                        currDevice = deviceStruct.next;
                    }
                } finally {
                    if (deviceList != IntPtr.Zero) {
                        LibFtdi.ftdi_list_free(ref deviceList);
                    }
                }
            }
        } finally {
            LibFtdi.ftdi_free(ftdi);
        }

        return (devices.AsReadOnly());
    }


    private static void GetUsbStrings(IntPtr usbDeviceHandle, out string manufacturer, out string description, out string serial) {
        var ftdi = LibFtdi.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        try {
            var sbManufacturer = new StringBuilder(256);
            var sbDescription = new StringBuilder(256);
            var sbSerial = new StringBuilder(256);

            var errorCode = LibFtdi.ftdi_usb_get_strings(ftdi, usbDeviceHandle,
                sbManufacturer, sbManufacturer.Capacity,
                sbDescription, sbDescription.Capacity,
                sbSerial, sbSerial.Capacity);
            ThrowIfError(ftdi, "ftdi_usb_get_strings", errorCode);

            manufacturer = sbManufacturer.ToString();
            description = sbDescription.ToString();
            serial = sbSerial.ToString();
        } finally {
            LibFtdi.ftdi_free(ftdi);
        }
    }

    /// <summary>
    /// Gets all EEPROM bytes without any processing.
    /// </summary>
    internal static byte[] GetRawEepromBytes(IntPtr usbDeviceHandle) {
        var ftdi = LibFtdi.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        try {
            ThrowIfError(ftdi, "ftdi_usb_open_dev", LibFtdi.ftdi_usb_open_dev(ftdi, usbDeviceHandle));

            try {
                var rawEepromBytes = new byte[4096];
                var len = LibFtdi.ftdi_read_eeprom_getsize(ftdi, rawEepromBytes, rawEepromBytes.Length);
                ThrowIfError(ftdi, "ftdi_read_eeprom", len);

                var eepromBytes = new byte[len];
                Buffer.BlockCopy(rawEepromBytes, 0, eepromBytes, 0, eepromBytes.Length);
                return eepromBytes;
            } finally {
                LibFtdi.ftdi_usb_close(ftdi);
            }
        } finally {
            LibFtdi.ftdi_free(ftdi);
        }
    }

    [StackTraceHidden()]
    internal static void ThrowIfError(IntPtr ftdi, string errorSource, int errorCode) {
        if (errorCode < 0) {
            var errorPointer = LibFtdi.ftdi_get_error_string(ftdi);
            if (errorPointer == IntPtr.Zero) {
                throw new InvalidOperationException(errorSource + " failed with error code " + errorCode.ToString() + ".");
            } else {
                var errorText = Marshal.PtrToStringUTF8(errorPointer);
                throw new InvalidOperationException(errorSource + " failed with error code " + errorCode.ToString() + ": " + errorText);
            }
        }
    }

}