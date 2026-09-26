using System.Runtime.InteropServices;

namespace DarkSunWakeRedux.Game;

internal static class StartupFailureReporter
{
    /// <param name="allowDialog">
    /// Whether this launch may block on a modal dialog. A smoke test or any other unattended run
    /// passes <c>false</c>: there is nobody to dismiss a message box, so showing one replaces a
    /// diagnosable non-zero exit with a hang that hides the very error it is reporting.
    /// </param>
    public static void Report(Exception exception, bool allowDialog = true)
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
        if (allowDialog && OperatingSystem.IsWindows() && !IsAutomated())
            _ = MessageBoxW(IntPtr.Zero, message, "Dark Sun: Wake of the Ravager Redux", 0x10);
    }

    // Deliberately only an explicit signal. This process is a WinExe, so a person launching it from
    // Explorer has no console and its standard handles look redirected. Inferring "unattended"
    // from those would take the dialog away from the one case it exists for.
    private static bool IsAutomated() =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

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
