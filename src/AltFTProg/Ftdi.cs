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
    /// Gets device manufacturer name.
    /// </summary>
    public string Manufacturer {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            GetEepromStrings(EepromBytes, out var manufacturer, out _, out _);
            return manufacturer;
        }
    }

    /// <summary>
    /// Gets device product name.
    /// </summary>
    public string Product {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            GetEepromStrings(EepromBytes, out _, out var product, out _);
            return product;
        }
    }

    /// <summary>
    /// Gets device serial number.
    /// </summary>
    public string Serial {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            GetEepromStrings(EepromBytes, out _, out _, out var serial);
            return serial;
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
    public FtdiChipType ChipType {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (FtdiChipType)EepromBytes[7];
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
    /// Gets/sets if USB 1.1 interface is used.
    /// TODO: Check bytes 12 and 13
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsUsb11 {
        get {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            return (EepromBytes[10] & 0x10) != 0;
        }
        set {
            if (EepromBytes == null) { EepromBytes = GetEepromBytes(); }
            if (!IsChecksumValid) { throw new InvalidOperationException("Current checksum is invalid."); }
            EepromBytes[10] = (byte)((EepromBytes[10] & ~0x10) | (value ? 0x10 : 0));
            IsChecksumValid = true;  // fixup checksum
        }
    }

    /// <summary>
    /// Gets/sets if USB 2.0 interface is used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Current checksum is invalid.</exception>
    public bool IsUsb20 {
        get { return !IsUsb11; }
        set { IsUsb11 = !value; }
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
        var ftdi = new NativeMethods.ftdi_context();
        try {
            var initRes = NativeMethods.ftdi_init(ref ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            try {
                NativeMethods.ftdi_usb_open_dev(ref ftdi, UsbDeviceHandle);

                var eeprom = new byte[4096];
                var eepromFullLen = NativeMethods.ftdi_read_eeprom_getsize(ref ftdi, eeprom, eeprom.Length);
                if (eepromFullLen < 0) { throw new InvalidOperationException("ftdi_read_eeprom failed with error code " + eepromFullLen.ToString()); }

                var eepromLen = 128;  // ftdi_read_eeprom_getsize returns more data than the actual EEPROM size; assume 128 bytes

                var eepromBytes = new byte[includeExtras ? eepromFullLen : eepromLen];  // function returns number of bytes read
                Buffer.BlockCopy(eeprom, 0, eepromBytes, 0, eepromBytes.Length);

                return eepromBytes;
            } finally {
                NativeMethods.ftdi_usb_close(ref ftdi);
            }
        } finally {
            NativeMethods.ftdi_deinit(ref ftdi);
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

        var ftdi = new NativeMethods.ftdi_context();
        try {
            var initRes = NativeMethods.ftdi_init(ref ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            try {
                NativeMethods.ftdi_usb_open_dev(ref ftdi, UsbDeviceHandle);

                var result = NativeMethods.ftdi_write_eeprom(ref ftdi, eepromBytes);
                if (result < 0) { throw new InvalidOperationException("ftdi_write_eeprom failed with error code " + result.ToString()); }
            } finally {
                NativeMethods.ftdi_usb_close(ref ftdi);
            }
        } finally {
            NativeMethods.ftdi_deinit(ref ftdi);
        }
    }


    private static void GetUsbStrings(IntPtr usbDeviceHandle, out string manufacturer, out string description, out string serial) {
        var ftdi = new NativeMethods.ftdi_context();
        try {
            var initRes = NativeMethods.ftdi_init(ref ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            var sbManufacturer = new StringBuilder(256);
            var sbDescription = new StringBuilder(256);
            var sbSerial = new StringBuilder(256);
            NativeMethods.ftdi_usb_get_strings(ref ftdi, usbDeviceHandle,
                sbManufacturer, sbManufacturer.Capacity,
                sbDescription, sbDescription.Capacity,
                sbSerial, sbSerial.Capacity);

            manufacturer = sbManufacturer.ToString();
            description = sbDescription.ToString();
            serial = sbSerial.ToString();
        } finally {
            NativeMethods.ftdi_deinit(ref ftdi);
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
        var ftdi = new NativeMethods.ftdi_context();
        var deviceList = IntPtr.Zero;

        try {
            var initRes = NativeMethods.ftdi_init(ref ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            var findRes = NativeMethods.ftdi_usb_find_all(ref ftdi, ref deviceList, 0x0403, 0x6001);
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
            NativeMethods.ftdi_deinit(ref ftdi);
            if (deviceList != IntPtr.Zero) {
                NativeMethods.ftdi_list_free(ref deviceList);
            }
        }
    }


    private static class NativeMethods {

        /** FTDI chip type */
        public enum ftdi_chip_type {
            TYPE_AM = 0,
            TYPE_BM = 1,
            TYPE_2232C = 2,
            TYPE_R = 3,
            TYPE_2232H = 4,
            TYPE_4232H = 5,
            TYPE_232H = 6
        };

        /** Automatic loading / unloading of kernel modules */
        public enum ftdi_module_detach_mode {
            AUTO_DETACH_SIO_MODULE = 0,
            DONT_DETACH_SIO_MODULE = 1
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


        [DllImport("libftdi")]
        public static extern void ftdi_deinit(
            ref ftdi_context ftdi
        );

        [DllImport("libftdi")]
        public static extern int ftdi_init(
            ref ftdi_context ftdi
        );

        [DllImport("libftdi")]
        public static extern void ftdi_list_free(
            ref IntPtr devlist
        );

        [DllImport("libftdi")]
        public static extern int ftdi_read_eeprom(
            ref ftdi_context ftdi,
            [Out] byte[] eeprom
        );

        [DllImport("libftdi")]
        public static extern int ftdi_read_eeprom_getsize(
            ref ftdi_context ftdi,
            [Out] byte[] eeprom,
            int maxsize
        );

        [DllImport("libftdi")]
        public static extern int ftdi_write_eeprom(
            ref ftdi_context ftdi,
            byte[] eeprom
        );

        [DllImport("libftdi")]
        public static extern int ftdi_usb_find_all(
            ref ftdi_context ftdi,
            ref IntPtr devlist,
            int vendor,
            int product
        );

        [DllImport("libftdi")]
        public static extern int ftdi_usb_close(
            ref ftdi_context ftdi
        );

        [DllImport("libftdi")]
        public static extern int ftdi_usb_get_strings(
            ref ftdi_context ftdi,
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
            ref ftdi_context ftdi,
            IntPtr dev
        );

    }
}


/// <summary>
/// FTDI chip type.
/// </summary>
public enum FtdiChipType {
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
    Ftdi232H = 6
};
