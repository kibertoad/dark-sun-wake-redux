using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class SourceCorpusInventoryTests
{
    [Fact]
    public void EmbeddedGogManifestProtectsTheCompleteImmutableCorpusContract()
    {
        var assembly = typeof(AssetPackInstaller).Assembly;
        var resourceName = Assert.Single(assembly.GetManifestResourceNames(),
            name => name.EndsWith(".json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName);
        Assert.NotNull(stream);
        var manifest = SourceManifest.Load(stream);

        Assert.Equal(OriginalContent.GameId, manifest.GameId);
        Assert.Equal(233, manifest.Files.Count);
        Assert.Equal(26, manifest.Files.Count(file =>
            file.Path.EndsWith(".GFF", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(147, manifest.Files.Count(file =>
            file.Path.EndsWith(".VOC", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(5, manifest.Files.Count(file =>
            file.Path.EndsWith(".FLI", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(40, manifest.Files.Count(file =>
            file.Path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)));
        var regionSources = RegionCatalogExtractor.SourcePaths(manifest);
        Assert.Equal(OriginalContent.RequiredStructuralRegionCatalogCount, regionSources.Count);
        Assert.Equal(
        [
            "RGN001.GFF", "RGN032.GFF", "RGN033.GFF", "RGN034.GFF", "RGN036.GFF",
            "RGN037.GFF", "RGN038.GFF", "RGN039.GFF", "RGN03A.GFF", "RGN03B.GFF",
            "RGN03C.GFF", "RGN03D.GFF", "RGN03E.GFF", "RGN03F.GFF", "RGN041.GFF",
            "RGN042.GFF", "RGN043.GFF", "RGN044.GFF", "RGN045.GFF", "RGN0FF.GFF"
        ], regionSources);
        Assert.Equal(regionSources.Count, regionSources
            .Select(OriginalContent.RegionCatalogAssetPathFor)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count());
        Assert.Equal(manifest.Files.Count, manifest.Files
            .Select(file => SourceManifest.Normalize(file.Path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count());
    }

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
                [new("DATA.GFF", 1, new string('0', 32))]);

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
                [new("DATA.GFF", 1, new string('0', 32))]);

            var inventory = SourceCorpusInventory.Read(root, manifest);

            Assert.False(inventory.IsComplete);
            Assert.Equal(SourceCorpusDisposition.Unrepresented,
                Assert.Single(inventory.Records).Disposition);
        }
        finally { Directory.Delete(root, true); }
    }
}
