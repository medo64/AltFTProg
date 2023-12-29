namespace AltFTProg;

using System;
using System.Text;

internal sealed class EepromStrings {

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="eepromBytes">EEPROM bytes.</param>
    /// <param name="pointersOffset">Offset to the first USB string pointer field.</param>
    /// <param name="pointersOffsetMask">Mask to apply to data pointers.</param>
    /// <param name="dataOffset">Offset where string data starts.</param>
    /// <param name="dataLength">Total length of string data.</param>
    public EepromStrings(byte[] eepromBytes, int pointersOffset, byte pointersOffsetMask, int dataOffset, int dataLength) {
        EepromBytes = eepromBytes;
        PointersOffset = pointersOffset;
        PointersOffsetMask = pointersOffsetMask;
        DataOffset = dataOffset;
        DataLength = dataLength;
    }

    private readonly byte[] EepromBytes;
    private readonly int PointersOffset;
    private readonly byte PointersOffsetMask;
    private readonly int DataOffset;
    private readonly int DataLength;


    /// <summary>
    /// Gets/sets manufacturer name.
    /// </summary>
    public string Manufacturer {
        get { return GetEepromString(EepromBytes, PointersOffset + 0, PointersOffsetMask); }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (value.Length == 0) { throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be empty."); }
            SetEepromStrings(EepromBytes, PointersOffset, PointersOffsetMask, DataOffset, DataLength,
                value, ProductDescription, SerialNumber);
        }
    }

    /// <summary>
    /// Gets/sets product description.
    /// </summary>
    public string ProductDescription {
        get { return GetEepromString(EepromBytes, PointersOffset + 2, PointersOffsetMask); }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (value.Length == 0) { throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be empty."); }
            SetEepromStrings(EepromBytes, PointersOffset, PointersOffsetMask, DataOffset, DataLength,
                Manufacturer, value, SerialNumber);
        }
    }

    /// <summary>
    /// Gets/sets serial number.
    /// </summary>
    public string SerialNumber {
        get { return GetEepromString(EepromBytes, PointersOffset + 4, PointersOffsetMask); }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            SetEepromStrings(EepromBytes, PointersOffset, PointersOffsetMask, DataOffset, DataLength,
                Manufacturer, ProductDescription, value);
        }
    }


    /// <summary>
    /// Sets all string descriptors at once.
    /// </summary>
    /// <param name="manufacturer">Manufacturer name.</param>
    /// <param name="productDescription">Product description.</param>
    /// <param name="serialNumber">Serial number.</param>
    public void SetEepromStrings(string manufacturer, string productDescription, string serialNumber) {
        SetEepromStrings(EepromBytes, PointersOffset, PointersOffsetMask, DataOffset, DataLength,
            manufacturer, productDescription, serialNumber);
    }

    /// <summary>
    /// Returns true if data fits in EEPROM.
    /// </summary>
    /// <param name="manufacturer">Manufacturer name.</param>
    /// <param name="productDescription">Product description.</param>
    /// <param name="serialNumber">Serial number.</param>
    public bool CheckUnicodeCharacterCount(string manufacturer, string productDescription, string serialNumber) {
        var manufacturerBytes = Encoding.Unicode.GetBytes(manufacturer);
        var productBytes = Encoding.Unicode.GetBytes(productDescription);
        var serialBytes = Encoding.Unicode.GetBytes(serialNumber);
        var hasSerial = (serialNumber.Length > 0);

        var manufacturerDataLength = 2 + manufacturerBytes.Length;
        var productDataLength = 2 + productBytes.Length;
        var serialDataLength = hasSerial ? (2 + serialBytes.Length) : 0;  // writen only if not empty

        var totalLength = 0;
        totalLength += manufacturerDataLength;
        totalLength += productDataLength;
        totalLength += serialDataLength;

        return (totalLength <= DataLength);
    }

    /// <summary>
    /// Returns number of unicode characters needed to encode value.
    /// </summary>
    /// <param name="value">String value.</param>
    public static int GetUnicodeCharacterCount(string value) {
        var bytes = Encoding.Unicode.GetBytes(value);
        return bytes.Length;
    }


    private static void SetEepromStrings(byte[] eepromBytes, int pointerOffset, byte pointerOffsetMask, int dataOffset, int dataLength, string manufacturer, string productDescription, string serialNumber) {
        var manufacturerBytes = Encoding.Unicode.GetBytes(manufacturer);
        var productBytes = Encoding.Unicode.GetBytes(productDescription);
        var serialBytes = Encoding.Unicode.GetBytes(serialNumber);
        var hasSerial = (serialNumber.Length > 0);

        var manufacturerDataLength = 2 + manufacturerBytes.Length;
        var productDataLength = 2 + productBytes.Length;
        var serialDataLength = hasSerial ? (2 + serialBytes.Length) : 0;  // writen only if not empty

        var totalLength = 0;
        totalLength += manufacturerDataLength;
        totalLength += productDataLength;
        totalLength += serialDataLength;
        if (totalLength > dataLength) { throw new InvalidOperationException("Not enough EEPROM space for USB string descriptors."); }

        var manufacturerDataOffset = dataOffset;
        var productDataOffset = manufacturerDataOffset + manufacturerDataLength;
        var serialDataOffset = hasSerial ? productDataOffset + productDataLength : 0;  // writen only if not empty
        var lastDataOffset = hasSerial ? serialDataOffset + serialDataLength : productDataOffset + productDataLength;

        eepromBytes[pointerOffset + 0] = (byte)(manufacturerDataOffset | ~pointerOffsetMask);
        eepromBytes[pointerOffset + 1] = (byte)manufacturerDataLength;
        eepromBytes[pointerOffset + 2] = (byte)(productDataOffset | ~pointerOffsetMask);
        eepromBytes[pointerOffset + 3] = (byte)productDataLength;
        eepromBytes[pointerOffset + 4] = (byte)(serialDataOffset | ~pointerOffsetMask);
        eepromBytes[pointerOffset + 5] = (byte)serialDataLength;

        eepromBytes[manufacturerDataOffset + 0] = (byte)manufacturerDataLength;
        eepromBytes[manufacturerDataOffset + 1] = 0x03;
        for (var i = 0; i < manufacturerBytes.Length; i++) {
            eepromBytes[manufacturerDataOffset + 2 + i] = manufacturerBytes[i];
        }

        eepromBytes[productDataOffset + 0] = (byte)productDataLength;
        eepromBytes[productDataOffset + 1] = 0x03;
        for (var i = 0; i < productBytes.Length; i++) {
            eepromBytes[productDataOffset + 2 + i] = productBytes[i];
        }

        if (hasSerial) {
            eepromBytes[serialDataOffset + 0] = (byte)serialDataLength;
            eepromBytes[serialDataOffset + 1] = 0x03;
            for (var i = 0; i < serialBytes.Length; i++) {
                eepromBytes[serialDataOffset + 2 + i] = serialBytes[i];
            }
        }

        for (var i = lastDataOffset; i < dataOffset + dataLength; i++) {
            eepromBytes[i] = 0x00;
        }
    }

    private static string GetEepromString(byte[] eepromBytes, int pointerOffset, byte pointerOffsetMask) {
        var dataOffset = eepromBytes[pointerOffset + 0] & pointerOffsetMask;
        var dataLength1 = eepromBytes[pointerOffset + 1];
        if (dataOffset == 0) {
            Helpers.WriteDebug($"No data for pointer at 0x{pointerOffset:X2}");
            return "";
        }
        if (dataLength1 == 0) {
            Helpers.WriteDebug($"Zero-length data for pointer at 0x{pointerOffset:X2}");
            return "";
        }

        var dataLength2 = eepromBytes[dataOffset];
        if (dataLength1 != dataLength2) { Helpers.WriteDebug($"EEPROM length mismatch (0x{dataLength1:X2} != 0x{dataLength2:X2}) for pointer at 0x{pointerOffset:X2} (0x{pointerOffset + 1:X2}, 0x{dataOffset:X2})"); }
        var dataLength = Math.Min(dataLength1, dataLength2);

        var dataType = eepromBytes[dataOffset + 1];
        if (dataType != 0x03) {
            Helpers.WriteDebug($"Unrecognized data type (0x{dataType:X2}) for pointer at 0x{pointerOffset:X2} (data at 0x{dataOffset:X2})");
            return "";
        }

        var dataBytes = new byte[dataLength - 2];
        Buffer.BlockCopy(eepromBytes, dataOffset + 2, dataBytes, 0, dataBytes.Length);
        return Encoding.Unicode.GetString(dataBytes);
    }

}