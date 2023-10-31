namespace AltFTProg;

/// <summary>
/// FTDI chip type.
/// </summary>
public enum FtdiDeviceType {
    /// <summary>
    /// Empty device.
    /// </summary>
    Empty = 0,

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
