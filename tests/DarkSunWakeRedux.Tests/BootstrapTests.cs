using System.Security.Cryptography;
using System.Text.Json;
using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class BootstrapTests
{
    [Fact]
    public void ManifestRejectsPathTraversal()
    {
        var manifest = new SourceManifest("game", "edition", [new("../outside", 0, new string('0', 64))]);
        Assert.Throws<InvalidDataException>(manifest.Validate);
    }

    [Fact]
    public async Task SourceIdentificationUsesExactFingerprint()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var sourceFile = Path.Combine(root, "GAME.DAT");
            await File.WriteAllBytesAsync(sourceFile, [1, 2, 3], TestContext.Current.CancellationToken);
            var hash = await HashAsync(sourceFile);
            var edition = new SourceManifest("game", "synthetic-edition", [new("GAME.DAT", 3, hash)]);

            var identification = await OriginalContent.IdentifyAsync(
                root, [edition], TestContext.Current.CancellationToken);

            Assert.Equal("synthetic-edition", identification.Edition?.SourceEdition);
            Assert.Empty(identification.Diagnostics);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task SourceIdentificationReportsStableMismatchCode()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var sourceFile = Path.Combine(root, "GAME.DAT");
            await File.WriteAllBytesAsync(sourceFile, [1, 2, 3], TestContext.Current.CancellationToken);
            var edition = new SourceManifest("game", "synthetic-edition",
                [new("GAME.DAT", 4, new string('0', 64))]);

            var identification = await OriginalContent.IdentifyAsync(
                root, [edition], TestContext.Current.CancellationToken);

            Assert.False(identification.IsSupported);
            Assert.Equal("source_size_mismatch", Assert.Single(identification.Diagnostics).Code);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task VerifiedAssetPackRequiresExactInventoryAndHashes()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var assetPath = Path.Combine(root, "images", "synthetic.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath)!);
            await File.WriteAllBytesAsync(assetPath, [4, 5, 6], TestContext.Current.CancellationToken);
            var manifest = new AssetPackManifest(
                OriginalContent.RequiredAssetPackRevision,
                OriginalContent.GameId,
                "synthetic-edition",
                new string('a', 64),
                "test",
                [new("images/synthetic.bin", 3, await HashAsync(assetPath), "SOURCE.GFF",
                    "application/octet-stream", "synthetic-test")]);
            await File.WriteAllTextAsync(Path.Combine(root, "manifest.json"),
                JsonSerializer.Serialize(manifest), TestContext.Current.CancellationToken);

            Assert.Empty(await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken));

            await File.WriteAllBytesAsync(Path.Combine(root, "unexpected.bin"), [7],
                TestContext.Current.CancellationToken);
            var diagnostics = await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken);
            Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "pack_asset_unexpected");
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task MissingAssetPackIsActionable()
    {
        var diagnostics = await OriginalContent.VerifyInstalledAsync(
            Path.Combine(TestRoot(), "missing"), TestContext.Current.CancellationToken);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("pack_manifest_missing", diagnostic.Code);
        Assert.Contains("DarkSunWakeRedux.Extractor", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CorpusScaleManifestLimitRejectsOnlyOversizedInput()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            await using (var stream = File.Create(Path.Combine(root, "manifest.json")))
                stream.SetLength(OriginalContent.MaximumManifestBytes + 1);

            var diagnostic = Assert.Single(await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken));

            Assert.Equal("pack_manifest_too_large", diagnostic.Code);
            Assert.Equal(OriginalContent.MaximumManifestBytes.ToString(), diagnostic.Expected);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task EarlierSelfConsistentAssetPackRevisionRequiresReplacement()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var assetPath = Path.Combine(root, "images", "synthetic.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath)!);
            await File.WriteAllBytesAsync(assetPath, [1], TestContext.Current.CancellationToken);
            var manifest = new AssetPackManifest(
                OriginalContent.RequiredAssetPackRevision - 1,
                OriginalContent.GameId,
                "synthetic-edition",
                new string('c', 64),
                "test",
                [new("images/synthetic.bin", 1, await HashAsync(assetPath), "SOURCE.GFF",
                    "application/octet-stream", "synthetic-test")]);
            await File.WriteAllTextAsync(Path.Combine(root, "manifest.json"),
                JsonSerializer.Serialize(manifest), TestContext.Current.CancellationToken);

            var diagnostic = Assert.Single(await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken));

            Assert.Equal("pack_revision_mismatch", diagnostic.Code);
            Assert.Equal(OriginalContent.RequiredAssetPackRevision.ToString(), diagnostic.Expected);
            Assert.Equal((OriginalContent.RequiredAssetPackRevision - 1).ToString(), diagnostic.Actual);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task AssetPackInstallIsVerifiedAndReplacesStaleOutput()
    {
        var root = TestRoot();
        var output = Path.Combine(root, "pack");
        Directory.CreateDirectory(output);
        await File.WriteAllTextAsync(Path.Combine(output, "stale.bin"), "stale",
            TestContext.Current.CancellationToken);
        try
        {
            await AssetPackInstaller.InstallAsync(output, async staging =>
            {
                var asset = Path.Combine(staging, "images", "synthetic.bin");
                Directory.CreateDirectory(Path.GetDirectoryName(asset)!);
                await File.WriteAllBytesAsync(asset, [8, 9], TestContext.Current.CancellationToken);
                return new AssetPackManifest(
                    OriginalContent.RequiredAssetPackRevision,
                    OriginalContent.GameId,
                    "synthetic-edition",
                    new string('b', 64),
                    "test",
                    [new("images/synthetic.bin", 2, await HashAsync(asset), "SOURCE.GFF",
                        "application/octet-stream", "synthetic-test")]);
            });

            Assert.False(File.Exists(Path.Combine(output, "stale.bin")));
            Assert.Empty(await OriginalContent.VerifyInstalledAsync(
                output, TestContext.Current.CancellationToken));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static string TestRoot() => Path.Combine(
        Path.GetTempPath(), "dark-sun-wake-redux-tests", Guid.NewGuid().ToString("N"));

    private static async Task<string> HashAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(await SHA256.HashDataAsync(
            stream, TestContext.Current.CancellationToken));
    }
}
