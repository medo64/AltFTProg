namespace AltFTProgGui;
using AltFTProg;

internal class DeviceItem {
    public DeviceItem(FtdiDevice device) {
        Device = device;

        string deviceTitle;
        if (string.IsNullOrEmpty(device.UsbSerialNumber)) {
            deviceTitle = "FTDI " + GetDeviceTypeShortText(device) + " (no serial number)";
        } else {
            deviceTitle = "FTDI " + GetDeviceTypeShortText(device) + " (" + device.UsbSerialNumber + ")";
        }
        Title = deviceTitle;
    }

    public FtdiDevice Device { get; }
    public string Title { get; }

    public bool HasChanged { get; set; }


    public override string ToString() {
        return Title;
    }


    private static string GetDeviceTypeShortText(FtdiDevice device) {
        if (device is Ftdi232RDevice) {
            return "232R";
        } else if (device is FtdiXSeriesDevice) {
            return "X Series";
        } else {
            var type = device.DeviceType;
            return type switch {
                FtdiDeviceType.FT232A => "232/245AM",
                FtdiDeviceType.FT232B => "232/245BM",
                FtdiDeviceType.FT2232D => "2232D",
                FtdiDeviceType.FT232R => "232R/245R",
                FtdiDeviceType.FT2232H => "2232H",
                FtdiDeviceType.FT232H => "232H",
                FtdiDeviceType.FTXSeries => "X Series",
                _ => "(unknown)",
            };
        }
    }
}