using System.Runtime.InteropServices;

namespace DarkSunWakeRedux.Game;

internal static class StartupFailureReporter
{
    public static void Report(Exception exception)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DarkSunWakeRedux", "Logs");
        string? log = null;
        try
        {
            Directory.CreateDirectory(root);
            log = Path.Combine(root, "startup-error.log");
            File.WriteAllText(log, $"{DateTimeOffset.UtcNow:O}{Environment.NewLine}{exception}");
        }
        catch { }
        var message = $"Dark Sun: Wake of the Ravager Redux could not start.{Environment.NewLine}{Environment.NewLine}" +
            exception.Message + (log is null ? "" : $"{Environment.NewLine}{Environment.NewLine}Technical details: {log}");
        Console.Error.WriteLine(message);
        Console.Error.WriteLine(exception);
        if (OperatingSystem.IsWindows()) _ = MessageBoxW(IntPtr.Zero, message, "Dark Sun: Wake of the Ravager Redux", 0x10);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr window, string text, string caption, uint type);
}
