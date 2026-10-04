using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using RefurbishedDinosaurs.Core.Diagnostics;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartupFailureReporterTests
{
    [Fact]
    public void IdentifiesAStaleInstalledAssetPackAndPreservesTheLogLocation()
    {
        var exception = new InvalidDataException("The installed interaction UI graph is incomplete.");
        var message = StartupFailure.BuildMessage(StartupFailureReporter.Options(exception), exception,
            null, "C:\\logs\\startup-error.log");

        Assert.Contains("Dark Sun: Wake of the Ravager Redux could not start.", message);
        Assert.Contains("The local asset pack is stale or incomplete.", message);
        Assert.Contains("Asset Extractor", message);
        Assert.Contains("Technical details: C:\\logs\\startup-error.log", message);
    }

    [Fact]
    public void NamesTheAssetPackItTried()
    {
        var exception = new InvalidDataException("A verified local asset pack is required.");
        var pack = Path.Combine(Path.GetTempPath(), "UserContent");
        var message = StartupFailure.BuildMessage(StartupFailureReporter.Options(exception), exception, pack);

        Assert.Contains($"Asset pack: {pack}", message);
        Assert.Contains("DarkSunWakeRedux.Extractor", message);
    }

    [Fact]
    public void DoesNotMisdiagnoseAnUnrelatedStartupFailureAsAContentPackFailure()
    {
        var exception = new InvalidOperationException("The graphics device could not initialize.");
        var message = StartupFailure.BuildMessage(StartupFailureReporter.Options(exception), exception, null);

        Assert.DoesNotContain("asset pack is stale", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The graphics device could not initialize.", message);
    }

    [Fact]
    public void WritesTheStartupLogUnderThePerUserStateDirectory()
    {
        var options = StartupFailureReporter.Options(new InvalidOperationException("x"));
        Assert.Equal(Path.Combine(OriginalContent.StateRoot(), "Logs"), options.LogDirectory);
    }
}
