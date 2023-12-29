namespace AltFTProg;
using System;
using System.Diagnostics;
using System.Text;

internal static class Helpers {

    internal static byte[] HexStringToByteArray(string hex) {
        var hexFiltered = new StringBuilder();
        foreach (var c in hex) {
            if (char.IsAsciiHexDigit(c)) {
                hexFiltered.Append(c);
            }
        }
        hex = hexFiltered.ToString();

        var bytes = new List<byte>();
        for(var i = 0; i < hex.Length; i += 2) {
            bytes.Add(Convert.ToByte(hex.Substring(i, 2), 16));
        }
        return bytes.ToArray();
    }

    [Conditional("DEBUG")]
    internal static void WriteDebug(string message) {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Console.ResetColor();
    }

}
