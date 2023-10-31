namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using AltFTProg.NativeMethods;

/// <summary>
/// FTDI device.
/// </summary>
public abstract class FtdiDevice {

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="usbDeviceHandle">LibUsb device handle.</param>
    /// <param name="usbVendorId">Vendor ID as retrieved from USB.</param>
    /// <param name="usbProductId">Product ID as retrieved from USB.</param>
    /// <param name="type">Assumed device type.</param>
    /// <param name="eepromBytes">EEPROM bytes.</param>
    /// <param name="eepromSize">Size of base EEPROM.</param>
    private protected FtdiDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes, int eepromSize) {
        UsbDeviceHandle = usbDeviceHandle;
        UsbVendorId = usbVendorId;
        UsbProductId = usbProductId;

        DeviceType = type;
        EepromBytes = eepromBytes;
        EepromSize = eepromSize;
    }

    private readonly IntPtr UsbDeviceHandle;

    /// <summary>
    /// EEPROM bytes.
    /// </summary>
    private protected readonly byte[] EepromBytes;


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
            if (_usbManufacturer == null) { GetUsbStrings(UsbDeviceHandle, out _usbManufacturer, out _usbProductDescription, out _usbSerialNumber); }
            return _usbManufacturer;
        }
    }

    private string? _usbProductDescription;
    /// <summary>
    /// Gets USB device product.
    /// This is read from the device and not from the EEPROM.
    /// </summary>
    public string UsbProductDescription {
        get {
            if (_usbProductDescription == null) { GetUsbStrings(UsbDeviceHandle, out _usbManufacturer, out _usbProductDescription, out _usbSerialNumber); }
            return _usbProductDescription;
        }
    }

    private string? _usbSerialNumber;
    /// <summary>
    /// Gets USB device serial number.
    /// This is read from the device and not from the EEPROM.
    /// </summary>
    public string UsbSerialNumber {
        get {
            if (_usbSerialNumber == null) { GetUsbStrings(UsbDeviceHandle, out _usbManufacturer, out _usbProductDescription, out _usbSerialNumber); }
            return _usbSerialNumber;
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
    /// Gets/sets if device is bus-powered.
    /// </summary>
    public bool BusPowered {
        get { return !SelfPowered; }
        set { SelfPowered = !value; }
    }

    /// <summary>
    /// Gets/sets if device is self-powered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual bool SelfPowered {
        get { return (EepromBytes[8] & 0x40) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets device power requirement.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual int MaxBusPower {
        get { return EepromBytes[9] * 2; }  // 2 mA unit
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets remote wakeup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual bool RemoteWakeupEnabled {
        get { return (EepromBytes[8] & 0x20) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets if IO pins will be pulled down in USB suspend.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual bool PulldownPinsInSuspend {
        get { return (EepromBytes[10] & 0x04) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
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
    public virtual string ProductDescription {
        get {
            Helpers.GetEepromStrings(EepromBytes, EepromSize, out _, out var product, out _);
            return product;
        }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets if serial number will be reported by device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual bool SerialNumberEnabled {
        get { return (EepromBytes[10] & 0x08) != 0; }
        set { throw new InvalidOperationException("Device not supported."); }
    }

    /// <summary>
    /// Gets/sets device serial number.
    /// </summary>
    /// <exception cref="InvalidOperationException">Device not supported.</exception>
    public virtual string SerialNumber {
        get {
            Helpers.GetEepromStrings(EepromBytes, EepromSize, out _, out _, out var serial);
            return serial;
        }
        set { throw new InvalidOperationException("Device not supported."); }
    }


    /// <summary>
    /// Gets/sets if checksum is valid.
    /// If checksum is not valid, it can be made valid by setting the property to true.
    /// </summary>
    /// <exception cref="ArgumentException">Checksum validity cannot be set to false.</exception>
    public bool IsChecksumValid {
        get {
            var eepromChecksum = (UInt16)(EepromBytes[EepromSize - 1] << 8 | EepromBytes[EepromSize - 2]);
            var checksum = Helpers.GetChecksum(EepromBytes, EepromSize);
            return (eepromChecksum == checksum);
        }
        set {
            if (value == false) { throw new ArgumentException("Checksum validity cannot be set to false.", nameof(value)); }
            var checksum = Helpers.GetChecksum(EepromBytes, EepromSize);
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
                var _ = LibFtdi.ftdi_usb_close(ftdi);
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
            var manufacturerBytes = new byte[128];
            var descriptionBytes = new byte[128];
            var serialBytes = new byte[128];

            var errorCode = LibFtdi.ftdi_usb_get_strings(ftdi, usbDeviceHandle,
                manufacturerBytes, manufacturerBytes.Length,
                descriptionBytes, descriptionBytes.Length,
                serialBytes, serialBytes.Length);
            ThrowIfError(ftdi, "ftdi_usb_get_strings", errorCode);

            manufacturer = Encoding.UTF8.GetString(manufacturerBytes);
            description = Encoding.UTF8.GetString(descriptionBytes);
            serial = Encoding.UTF8.GetString(serialBytes);
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
                var _ = LibFtdi.ftdi_usb_close(ftdi);
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
                throw new InvalidOperationException(errorSource + " failed with error code " + errorCode.ToString(CultureInfo.InvariantCulture) + ".");
            } else {
                var errorText = Marshal.PtrToStringUTF8(errorPointer);
                throw new InvalidOperationException(errorSource + " failed with error code " + errorCode.ToString(CultureInfo.InvariantCulture) + ": " + errorText);
            }
        }
    }

}
