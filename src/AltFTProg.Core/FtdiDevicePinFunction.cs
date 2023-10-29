namespace AltFTProg;

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
