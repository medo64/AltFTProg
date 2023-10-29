namespace AltFTProg;
using System;
using System.Diagnostics;
using System.Text;

/// <summary>
/// FTDI 232R device.
/// </summary>
internal static class Helpers {

    internal static void GetEepromStrings(byte[] eepromBytes, int eepromSize, out string manufacturer, out string product, out string serial) {
        if ((eepromBytes[0x0E] & 0x80) == 0) { throw new InvalidOperationException("Manufacturer EEPROM field not detected."); }
        if ((eepromBytes[0x10] & 0x80) == 0) { throw new InvalidOperationException("Product EEPROM field not detected."); }
        if ((eepromBytes[0x12] & 0x80) == 0) { throw new InvalidOperationException("Serial EEPROM field not detected."); }

        var offsetManufacturer = eepromBytes[0x0E];
        var offsetProduct = eepromBytes[0x10];
        var offsetSerial = eepromBytes[0x12];
        if (eepromSize < 256) {  // at least on FT232R, first bit is high and not part of offset
            offsetManufacturer &= 0x7F;
            offsetProduct &= 0x7F;
            offsetSerial &= 0x7F;
        }

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

        if (offsetManufacturer + len1Manufacturer >= eepromSize) { throw new InvalidOperationException("Manufacturer EEPROM field outside of bound."); }
        if (offsetProduct + len1Product >= eepromSize) { throw new InvalidOperationException("Product EEPROM field outside of bound."); }
        if (offsetSerial + len1Serial >= eepromSize) { throw new InvalidOperationException("Serial EEPROM field outside of bound."); }

        if (eepromBytes[offsetManufacturer + 1] != 0x03) { throw new InvalidOperationException("Manufacturer EEPROM field type mismatch."); }
        if (eepromBytes[offsetProduct + 1] != 0x03) { throw new InvalidOperationException("Product EEPROM field type mismatch."); }
        if (eepromBytes[offsetSerial + 1] != 0x03) { throw new InvalidOperationException("Serial EEPROM field type mismatch."); }

        manufacturer = GetEepromString(eepromBytes, offsetManufacturer, len1Manufacturer);
        product = GetEepromString(eepromBytes, offsetProduct, len1Product);
        serial = GetEepromString(eepromBytes, offsetSerial, len1Serial);
    }

    internal static string GetEepromString(byte[] eepromBytes, int offset, int length) {
        var stringBytes = new byte[length - 2];
        Buffer.BlockCopy(eepromBytes, offset + 2, stringBytes, 0, stringBytes.Length);
        return Encoding.Unicode.GetString(stringBytes);
    }

    internal static void SetEepromStrings(byte[] eepromBytes, int eepromSize, string manufacturer, string product, string serial) {
        var manufacturerBytes = Encoding.Unicode.GetBytes(manufacturer);
        var productBytes = Encoding.Unicode.GetBytes(product);
        var serialBytes = Encoding.Unicode.GetBytes(serial);

        var lenManufacturer = manufacturerBytes.Length + 2;
        var lenProduct = productBytes.Length + 2;
        var lenSerial = serialBytes.Length + 2;

        var offsetFirst = eepromBytes[0x0E];
        if (eepromSize < 256) { offsetFirst &= 0x7F; }  // at least on FT232R, first bit is high and not part of offset

        var offsetManufacturer = offsetFirst;
        var offsetProduct = offsetManufacturer + lenManufacturer;
        var offsetSerial = offsetProduct + lenProduct;
        var offsetLast = offsetSerial + lenSerial;
        if (offsetLast > (eepromSize - 2)) { throw new InvalidOperationException("Not enough memory to write USB strings."); }

        eepromBytes[0x0E] = (byte)(offsetManufacturer);
        eepromBytes[0x0F] = (byte)(lenManufacturer);
        eepromBytes[0x10] = (byte)(offsetProduct);
        eepromBytes[0x11] = (byte)(lenProduct);
        eepromBytes[0x12] = (byte)(offsetSerial);
        eepromBytes[0x13] = (byte)(lenSerial);
        if (eepromSize < 256) {  // at least on FT232R, first bit is high and not part of offset
            eepromBytes[0x0E] |= 0x80;
            eepromBytes[0x10] |= 0x80;
            eepromBytes[0x12] |= 0x80;
        }

        eepromBytes[offsetManufacturer + 0] = (byte)lenManufacturer;
        eepromBytes[offsetManufacturer + 1] = 0x03;
        for (var i = 0; i < manufacturerBytes.Length; i++) {
            eepromBytes[offsetManufacturer + 2 + i] = manufacturerBytes[i];
        }

        eepromBytes[offsetProduct + 0] = (byte)lenProduct;
        eepromBytes[offsetProduct + 1] = 0x03;
        for (var i = 0; i < productBytes.Length; i++) {
            eepromBytes[offsetProduct + 2 + i] = productBytes[i];
        }

        eepromBytes[offsetSerial + 0] = (byte)lenSerial;
        eepromBytes[offsetSerial + 1] = 0x03;
        for (var i = 0; i < serialBytes.Length; i++) {
            eepromBytes[offsetSerial + 2 + i] = serialBytes[i];
        }

        for (var i = offsetLast; i < eepromSize - 2; i++) {
            eepromBytes[i] = 0;
        }
    }

    internal static ushort GetChecksum(byte[] eeprom, int eepromSize, FtdiDeviceType device) {
        UInt16 crc = 0xAAAA;
        for (var i = 0; i < eepromSize - 2; i += 2) {
            if ((device == FtdiDeviceType.FTXSeries) && (i == 36)) { i = 128; }
            crc ^= (UInt16)(eeprom[i] | (eeprom[i + 1] << 8));
            crc = (UInt16)((crc << 1) | (crc >> 15));
        }
        return crc;
    }

    internal static int CountUnicodeChars(params string[] values) {
        var byteCount = 0;
        foreach (var value in values) {
            byteCount += Encoding.Unicode.GetByteCount(value);
        }
        return byteCount;
    }

}
