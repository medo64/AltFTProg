namespace AltFTProg;
using System;

/// <summary>
/// Unknown FTDI device.
/// </summary>
public sealed class FtdiUnknownDevice : FtdiDevice {

    internal FtdiUnknownDevice(IntPtr usbDeviceHandle, int usbVendorId, int usbProductId, FtdiDeviceType type, byte[] eepromBytes)
        : base(usbDeviceHandle, usbVendorId, usbProductId, type, eepromBytes) {
    }

}
