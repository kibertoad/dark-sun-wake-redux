using DarkSunWakeRedux.Resources;
using RefurbishedDinosaurs.Core.Diagnostics;

namespace DarkSunWakeRedux.Game;

/// <summary>
/// Reports a failed start through the toolkit's <see cref="StartupFailure"/>, with this game's title,
/// log directory and recovery advice.
/// </summary>
internal static class StartupFailureReporter
{
    private const string Title = "Dark Sun: Wake of the Ravager Redux";

    /// <param name="unattended">
    /// Whether nobody is there to dismiss a modal dialog, such as a smoke test. Showing one would then
    /// replace a diagnosable non-zero exit with a hang that hides the very error it is reporting.
    /// </param>
    public static void Report(Exception exception, string? assetPack, bool unattended)
    {
        // Only an explicit signal counts as unattended. This process is a WinExe, so a person
        // launching it from Explorer has no console and its standard handles look redirected;
        // inferring "unattended" from those would take the dialog away from the one case it exists for.
        var showDialog = !unattended && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
        StartupFailure.Report(Options(exception), exception, assetPack, showDialog);
    }

    internal static StartupFailureOptions Options(Exception exception) =>
        new(Title, Path.Combine(StateRootOrTemp(), "Logs"), RecoveryInstruction(exception), "Asset pack");

    // A runtime reader names a pack file it cannot use with "The installed ...", which means the
    // pack predates the current Extractor or lost a file after verification.
    internal static string RecoveryInstruction(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidDataException &&
            exception.Message.StartsWith("The installed ", StringComparison.Ordinal)
            ? "The local asset pack is stale or incomplete. Refresh it with the current Asset Extractor " +
              "against your legally owned original installation, then start the game again."
            : "If the asset pack is missing or damaged, run DarkSunWakeRedux.Extractor against your " +
              "legally owned GOG installation.";
    }

    // The failure being reported may be the one that stopped the state directory from resolving.
    private static string StateRootOrTemp()
    {
        try { return OriginalContent.StateRoot(); }
        catch (InvalidOperationException) { return Path.Combine(Path.GetTempPath(), "DarkSunWakeRedux"); }
    }
}
