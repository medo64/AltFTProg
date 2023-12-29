namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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
    private protected FtdiDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes) {
        UsbDeviceHandle = usbDeviceHandle;
        UsbVendorId = usbVendorId;
        UsbProductId = usbProductId;

        DeviceType = type;

        EepromBytes = eepromBytes;
        OriginalEepromBytes = new byte[eepromBytes.Length];
        Buffer.BlockCopy(eepromBytes, 0, OriginalEepromBytes, 0, OriginalEepromBytes.Length);
    }


    /// <summary>
    /// Gets device type.
    /// </summary>
    public FtdiDeviceType DeviceType { get; }

    /// <summary>
    /// Gets EEPROM size.
    /// </summary>
    public int EepromSize { get { return EepromBytes.Length; } }


    #region USB Properties

    private readonly IntPtr UsbDeviceHandle;

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


    #region EEPROM

    /// <summary>
    /// EEPROM bytes.
    /// </summary>
    private protected readonly byte[] EepromBytes;

    private readonly byte[] OriginalEepromBytes;

    /// <summary>
    /// Returns if EEPROM has changed.
    /// </summary>
    public bool HasEepromChanged {
        get {
            for (var i = 0; i < EepromBytes.Length; i++) {
                if (EepromBytes[i] != OriginalEepromBytes[i]) { return true; }
            }
            return false;
        }
    }

    /// <summary>
    /// Returns all EEPROM bytes for the USB device.
    /// </summary>
    public byte[] GetEepromBytes() {
        var eepromBytes = new byte[EepromBytes.Length];
        Buffer.BlockCopy(EepromBytes, 0, eepromBytes, 0, eepromBytes.Length);
        return eepromBytes;
    }

    /// <summary>
    /// Restores EEPROM state.
    /// </summary>
    public void ResetEeprom() {
        Buffer.BlockCopy(OriginalEepromBytes, 0, EepromBytes, 0, EepromBytes.Length);
    }

    /// <summary>
    /// Write any changes to EEPROM.
    /// </summary>
    public void SaveEeprom() {
        var ftdi = LibFtdi.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        try {
            ThrowIfError(ftdi, "ftdi_usb_open_dev", LibFtdi.ftdi_usb_open_dev(ftdi, UsbDeviceHandle));

            try {
                // some devices don't write all bytes properly (XSeries)
                for (var i = 0; i < EepromSize / 2; i++) {  // ftdi_write_eeprom doesn't work for 256 byte EEPROMs
                    var value = (ushort)((EepromBytes[i * 2 + 1] << 8) | EepromBytes[i * 2]);
                    var writeLocRes = LibFtdi.ftdi_write_eeprom_location(ftdi, i, value);
                    ThrowIfError(ftdi, "ftdi_write_eeprom_location", writeLocRes);
                }

                // some devices don't write all bytes properly (FT232)
                var writeRes = LibFtdi.ftdi_write_eeprom(ftdi, EepromBytes);
                ThrowIfError(ftdi, "ftdi_write_eeprom", writeRes);

            } finally {
                var _ = LibFtdi.ftdi_usb_close(ftdi);
            }
        } finally {
            LibFtdi.ftdi_free(ftdi);
        }
    }

    #endregion EEPROM


    #region Overrrides

    /// <summary>
    /// Returns true if two devices have matching USB handle.
    /// </summary>
    /// <param name="obj">Other object.</param>
    public override bool Equals(object? obj) {
        if (obj is FtdiDevice other) {
            return (UsbDeviceHandle == other.UsbDeviceHandle);
        }
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
        return UsbDeviceHandle.GetHashCode();
    }

    #endregion Overrrides


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

    private static ReadOnlyCollection<FtdiDevice> GetDevices(IEnumerable<KeyValuePair<int, int>> vidPids) {
        var ftdi = LibFtdi.ftdi_new();
        if (ftdi == IntPtr.Zero) { throw new InvalidOperationException("ftdi_new failed."); }

        var devices = new List<FtdiDevice>();

        try {
            foreach (var vidPid in vidPids) {
                Helpers.WriteDebug($"Processing VID={vidPid.Key:X4} PID={vidPid.Value:X4}");
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
                        for (; i >= 128; i--) {
                            if (rawEepromBytes[i] != 0xFF) { break; }
                        }
                        var nonEmptyBlockCount = i / 128 + 1;
                        Helpers.WriteDebug($"rawEepromBytes {nonEmptyBlockCount * 128} bytes ({nonEmptyBlockCount} blocks)");

                        var type = (FtdiDeviceType)((rawEepromBytes[7] << 8) | rawEepromBytes[6]);
                        if ((type == 0) && (rawEepromBytes.Length == 256)) { type = FtdiDeviceType.FT232R; }  // guess

                        FtdiDevice device;
                        switch (type) {
                            case FtdiDeviceType.FT232R: {
                                    var eepromBytes = new byte[256];
                                    Buffer.BlockCopy(rawEepromBytes, 0, eepromBytes, 0, Math.Min(eepromBytes.Length, rawEepromBytes.Length));
                                    device = new Ftdi232RDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes);
                                }
                                break;
                            case FtdiDeviceType.FTXSeries: {
                                    var eepromBytes = new byte[256];
                                    Buffer.BlockCopy(rawEepromBytes, 0, eepromBytes, 0, Math.Min(eepromBytes.Length, rawEepromBytes.Length));
                                    device = new FtdiXSeriesDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes);
                                }
                                break;

                            default: {
                                        var eepromExtraSize = nonEmptyBlockCount * 128;
                                        var eepromBytes = new byte[eepromExtraSize];
                                        Buffer.BlockCopy(rawEepromBytes, 0, eepromBytes, 0, Math.Min(eepromBytes.Length, rawEepromBytes.Length));
                                        device = new FtdiUnknownDevice(deviceStruct.dev, vendorId, productId, type, eepromBytes);
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
                Helpers.WriteDebug($"ftdi_read_eeprom_getsize len={len}");
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
