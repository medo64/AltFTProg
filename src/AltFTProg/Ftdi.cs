namespace AltFTProg;
using System;
using System.Collections;
using System.Collections.Generic;
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


    private string? _manufacturer;
    /// <summary>
    /// Gets USB device manufacturer name.
    /// </summary>
    public string Manufacturer {
        get {
            if (_manufacturer == null) { GetUsbStrings(UsbDeviceHandle, out _manufacturer, out _description, out _serial); }
            return _manufacturer;
        }
    }

    private string? _description;
    /// <summary>
    /// Gets USB device description.
    /// </summary>
    public string Description {
        get {
            if (_description == null) { GetUsbStrings(UsbDeviceHandle, out _manufacturer, out _description, out _serial); }
            return _description;
        }
    }

    private string? _serial;
    /// <summary>
    /// Gets USB device serial number.
    /// </summary>
    public string Serial {
        get {
            if (_serial == null) { GetUsbStrings(UsbDeviceHandle, out _manufacturer, out _description, out _serial); }
            return _serial;
        }
    }


    /// <summary>
    /// Returns all EEPROM bytes for the USB device.
    /// </summary>
    public byte[] GetRawEepromBytes() {
        var ftdi = new NativeMethods.ftdi_context();
        try {
            var initRes = NativeMethods.ftdi_init(ref ftdi);
            if (initRes < 0) { throw new InvalidOperationException("ftdi_init failed with error code " + initRes.ToString()); }

            try {
                NativeMethods.ftdi_usb_open_dev(ref ftdi, UsbDeviceHandle);

                var eeprom = new byte[4096];
                //int result = NativeMethods.ftdi_read_eeprom(ref ftdi, eepromData);
                var result = NativeMethods.ftdi_read_eeprom_getsize(ref ftdi, eeprom, eeprom.Length);
                if (result < 0) { throw new InvalidOperationException("ftdi_read_eeprom failed with error code " + result.ToString()); }

                var eepromBytes = new byte[result];  // function returns number of bytes read
                Buffer.BlockCopy(eeprom, 0, eepromBytes, 0, eepromBytes.Length);

                return eepromBytes;
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
                var device = (NativeMethods.ftdi_device_list)Marshal.PtrToStructure(currDevice, typeof(NativeMethods.ftdi_device_list))!;
                devices.Add(new FtdiDevice(device.dev));
                currDevice = device.next;
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
