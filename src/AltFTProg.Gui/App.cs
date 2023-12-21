namespace AltFTProgGui;
using System;
using Avalonia;

internal static class App {

    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<AppGui>()
            .UsePlatformDetect()
            .LogToTrace();
}
