namespace AltFTProgGui;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;

internal static class App {

    [STAThread]
    public static void Main(string[] args) {
        var isSudoInstance = ((args.Length >= 1) && args[0].Equals("--no-sudo", StringComparison.Ordinal));

        if (!isSudoInstance && (Environment.ProcessPath != null)) {
            var envUser = Environment.GetEnvironmentVariable("USER") ?? "";
            bool isRoot = envUser.Equals("root", StringComparison.Ordinal);

            if (!isRoot) {
                var envDisplay = Environment.GetEnvironmentVariable("DISPLAY") ?? "";
                var envXauthority = Environment.GetEnvironmentVariable("XAUTHORITY") ?? "";
                var sudoProcess = new ProcessStartInfo() {
                    FileName = "pkexec",
                    ArgumentList = {
                        "env",
                        "DISPLAY=" + envDisplay,
                        "XAUTHORITY=" + envXauthority,
                        Environment.ProcessPath,
                        "--no-sudo"
                    }
                };
                try {
                    var process = Process.Start(sudoProcess);
                    if (process != null) {
                        process.WaitForExit();
                        if (process.ExitCode == 0) { return; }  // successfully started sudo instance
                    }
                } catch (Win32Exception) { }  // if there is no pkexec
            }
        }

        // just start the app
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<AppGui>()
            .UsePlatformDetect()
            .LogToTrace();
}
