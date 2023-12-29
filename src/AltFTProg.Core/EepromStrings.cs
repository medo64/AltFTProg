namespace AltFTProg;

using System;
using System.Text;

internal sealed class EepromStrings {

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
    internal readonly int DataLength;


    public string Manufacturer {
        get { return GetEepromString(EepromBytes, PointersOffset + 0, PointersOffsetMask); }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (value.Length == 0) { throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be empty."); }
            SetEepromStrings(EepromBytes, PointersOffset, PointersOffsetMask, DataOffset, DataLength,
                value, ProductDescription, SerialNumber);
        }
    }

    public string ProductDescription {
        get { return GetEepromString(EepromBytes, PointersOffset + 2, PointersOffsetMask); }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            if (value.Length == 0) { throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be empty."); }
            SetEepromStrings(EepromBytes, PointersOffset, PointersOffsetMask, DataOffset, DataLength,
                Manufacturer, value, SerialNumber);
        }
    }

    public string SerialNumber {
        get { return GetEepromString(EepromBytes, PointersOffset + 4, PointersOffsetMask); }
        set {
            if (value == null) { throw new ArgumentNullException(nameof(value), "Value cannot be null."); }
            SetEepromStrings(EepromBytes, PointersOffset, PointersOffsetMask, DataOffset, DataLength,
                Manufacturer, ProductDescription, value);
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

}