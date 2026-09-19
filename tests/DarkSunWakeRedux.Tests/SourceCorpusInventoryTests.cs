using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class SourceCorpusInventoryTests
{
    [Fact]
    public async Task AssignsEveryKnownSourceFileAnExplicitDisposition()
    {
        var root = Path.Combine(Path.GetTempPath(), "dark-sun-wake-redux-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "capture"));
        Directory.CreateDirectory(Path.Combine(root, "DOSBOX"));
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(root, "DATA.GFF"), [1],
                TestContext.Current.CancellationToken);
            await File.WriteAllBytesAsync(Path.Combine(root, "capture", "latest.png"), [2],
                TestContext.Current.CancellationToken);
            await File.WriteAllBytesAsync(Path.Combine(root, "DOSBOX", "DOSBox.exe"), [3],
                TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(root, "Manual.pdf"), "manual",
                TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(root, "unins000.exe"), "uninstaller",
                TestContext.Current.CancellationToken);
            var manifest = new SourceManifest("game", "edition",
                [new("DATA.GFF", 1, new string('0', 64))]);

            var inventory = SourceCorpusInventory.Read(root, manifest);

            Assert.True(inventory.IsComplete);
            Assert.Equal(SourceCorpusDisposition.ImmutableGameData,
                Assert.Single(inventory.Records, record => record.Path == "DATA.GFF").Disposition);
            Assert.Equal(SourceCorpusDisposition.MutableCaptureOrSave,
                Assert.Single(inventory.Records, record => record.Path == "capture/latest.png").Disposition);
            Assert.Equal(SourceCorpusDisposition.DosBoxWrapper,
                Assert.Single(inventory.Records, record => record.Path == "DOSBOX/DOSBox.exe").Disposition);
            Assert.Equal(SourceCorpusDisposition.Documentation,
                Assert.Single(inventory.Records, record => record.Path == "Manual.pdf").Disposition);
            Assert.Equal(SourceCorpusDisposition.StorefrontWrapper,
                Assert.Single(inventory.Records, record => record.Path == "unins000.exe").Disposition);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task FlagsUnknownFileForExplicitDisposition()
    {
        var root = Path.Combine(Path.GetTempPath(), "dark-sun-wake-redux-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(root, "UNKNOWN.BIN"), [1],
                TestContext.Current.CancellationToken);
            var manifest = new SourceManifest("game", "edition",
                [new("DATA.GFF", 1, new string('0', 64))]);

            var inventory = SourceCorpusInventory.Read(root, manifest);

            Assert.False(inventory.IsComplete);
            Assert.Equal(SourceCorpusDisposition.Unrepresented,
                Assert.Single(inventory.Records).Disposition);
        }
        finally { Directory.Delete(root, true); }
    }
}
