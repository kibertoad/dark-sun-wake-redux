using DarkSunWakeRedux.Game;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartupFailureReporterTests
{
    [Fact]
    public void IdentifiesAStaleInstalledAssetPackAndPreservesTheLogLocation()
    {
        var message = StartupFailureReporter.BuildMessage(
            new InvalidDataException("The installed interaction UI graph is incomplete."),
            "C:\\logs\\startup-error.log");

        Assert.Contains("The local asset pack is stale or incomplete.", message);
        Assert.Contains("Asset Extractor", message);
        Assert.Contains("Technical details: C:\\logs\\startup-error.log", message);
    }

    [Fact]
    public void DoesNotMisdiagnoseAnUnrelatedStartupFailureAsAContentPackFailure()
    {
        var message = StartupFailureReporter.BuildMessage(
            new InvalidOperationException("The graphics device could not initialize."), null);

        Assert.DoesNotContain("asset pack is stale", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The graphics device could not initialize.", message);
    }
}
