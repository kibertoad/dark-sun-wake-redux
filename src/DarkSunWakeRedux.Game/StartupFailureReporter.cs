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
        var message = BuildMessage(exception, log);
        Console.Error.WriteLine(message);
        Console.Error.WriteLine(exception);
        if (OperatingSystem.IsWindows()) _ = MessageBoxW(IntPtr.Zero, message, "Dark Sun: Wake of the Ravager Redux", 0x10);
    }

    internal static string BuildMessage(Exception exception, string? log)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var refreshHint = exception is InvalidDataException &&
            exception.Message.StartsWith("The installed ", StringComparison.Ordinal)
            ? $"{Environment.NewLine}{Environment.NewLine}The local asset pack is stale or incomplete. " +
              "Refresh it with the current Asset Extractor against your legally owned original " +
              "installation, then start the game again."
            : string.Empty;
        return $"Dark Sun: Wake of the Ravager Redux could not start." +
            $"{Environment.NewLine}{Environment.NewLine}{exception.Message}{refreshHint}" +
            (log is null ? "" : $"{Environment.NewLine}{Environment.NewLine}Technical details: {log}");
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr window, string text, string caption, uint type);
}
