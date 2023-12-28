namespace AltFTProg.NativeMethods;
using System;
using System.Runtime.InteropServices;
using System.Text;

internal static class LibFtdi {

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
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern void ftdi_free(
        IntPtr ftdi
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern IntPtr ftdi_new(
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern void ftdi_list_free(
        ref IntPtr devlist
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern int ftdi_read_eeprom_getsize(
        IntPtr ftdi,
        [Out] byte[] eeprom,
        int maxsize
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern int ftdi_write_eeprom(
        IntPtr ftdi,
        byte[] eeprom
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern int ftdi_write_eeprom_location(
        IntPtr ftdi,
        int eeprom_addr,
        ushort eeprom_val
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern int ftdi_usb_find_all(
        IntPtr ftdi,
        ref IntPtr devlist,
        int vendor,
        int product
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern int ftdi_usb_close(
        IntPtr ftdi
    );

    [DllImport("libftdi.so", CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern int ftdi_usb_get_strings(
        IntPtr ftdi,
        IntPtr dev,
        [Out] byte[] manufacturer,
        int mnf_len,
        [Out] byte[] description,
        int desc_len,
        [Out] byte[] serial,
        int serial_len
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern int ftdi_usb_open_dev(
        IntPtr ftdi,
        IntPtr dev
    );

    [DllImport("libftdi.so")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern IntPtr ftdi_get_error_string(
        IntPtr ftdi
    );
}
