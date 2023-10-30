namespace AltFTProg;
using System;
using System.Text;

internal static class Helpers {

    public static string GetRandomSerial(string prefix, int digitCount) {
        var sb = new StringBuilder(prefix);
        for (var i = 0; i < digitCount; i++) {
            var number = Random.Shared.Next(0, 32);
            var ch = (number < 26) ? (char)('A' + number) : (char)('2' + (number - 26));
            sb.Append(ch);
        }
        return sb.ToString();
    }

}