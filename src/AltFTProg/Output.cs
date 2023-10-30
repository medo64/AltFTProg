namespace AltFTProg;
using System;

internal static class Output {
    public static void WriteLine(string text) {
        Console.WriteLine(text);
    }

    public static void WriteVerboseLine(string text) {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteWarningLine(string text) {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteErrorLine(string text) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(text);
        Console.ResetColor();
    }
}
