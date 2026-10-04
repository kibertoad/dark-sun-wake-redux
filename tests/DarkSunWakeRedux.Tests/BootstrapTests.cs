using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using RefurbishedDinosaurs.Core.Assets;
using RefurbishedDinosaurs.LegacyFormats;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class BootstrapTests
{
    [Fact]
    public void ManifestRejectsPathTraversal()
    {
        var manifest = new AssetManifest("game", "edition", [new("../outside", 0, new string('0', 32))]);
        Assert.Throws<InvalidDataException>(manifest.Validate);
    }

    [Fact]
    public void SupportedEditionManifestLoadsAsThisGamesDirectoryEdition()
    {
        var assembly = typeof(AssetPackInstaller).Assembly;
        var name = Assert.Single(assembly.GetManifestResourceNames(),
            resource => resource.EndsWith(".json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name)!;

        var edition = OriginalContent.LoadEdition(stream);

        Assert.Equal(OriginalContent.GameId, edition.GameId);
        Assert.Equal(ContentSourceKinds.Directory, edition.SourceKind);
        Assert.All(edition.Files, file => Assert.True(FileFingerprint.IsXxh3(file.Xxh3)));
    }

    [Fact]
    public void EditionManifestForAnotherGameIsRejected()
    {
        using var stream = new MemoryStream(
            """{"gameId":"other-game","sourceEdition":"e","files":[{"path":"A","size":1,"xxh3":"00000000000000000000000000000000"}]}"""u8.ToArray());
        Assert.Throws<InvalidDataException>(() => OriginalContent.LoadEdition(stream));
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
            var edition = new AssetManifest("game", "synthetic-edition", [new("GAME.DAT", 3, hash)]);

            var identification = await AssetVerifier.IdentifyAsync(
                root, [edition], TestContext.Current.CancellationToken);

            Assert.Equal("synthetic-edition", identification.Edition?.SourceEdition);
            Assert.Empty(identification.Mismatches);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task SourceIdentificationReportsStableMismatchProblem()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var sourceFile = Path.Combine(root, "GAME.DAT");
            await File.WriteAllBytesAsync(sourceFile, [1, 2, 3], TestContext.Current.CancellationToken);
            var edition = new AssetManifest("game", "synthetic-edition",
                [new("GAME.DAT", 4, new string('0', 32))]);

            var identification = await AssetVerifier.IdentifyAsync(
                root, [edition], TestContext.Current.CancellationToken);

            Assert.False(identification.IsSupported);
            var issue = Assert.Single(Assert.Single(identification.Mismatches).Issues);
            Assert.Equal(AssetProblem.WrongSize, issue.Problem);
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
            Manifest(OriginalContent.RequiredAssetPackRevision, 'a',
                    Asset("images/synthetic.bin", 3, await HashAsync(assetPath)))
                .Write(Path.Combine(root, OriginalContent.AssetPackManifestFileName));

            Assert.True((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).IsValid);

            await File.WriteAllBytesAsync(Path.Combine(root, "unexpected.bin"), [7],
                TestContext.Current.CancellationToken);
            var verification = await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken);
            Assert.Contains(verification.Issues, issue =>
                issue.Problem == InstalledAssetProblem.Unlisted && issue.Path == "unexpected.bin");

            File.Delete(Path.Combine(root, "unexpected.bin"));
            await File.WriteAllBytesAsync(assetPath, [4, 5, 7], TestContext.Current.CancellationToken);
            Assert.Equal(InstalledAssetProblem.WrongHash, Assert.Single((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).Issues).Problem);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task AssetPathEscapingThePackIsUnsafe()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            Manifest(OriginalContent.RequiredAssetPackRevision, 'a', Asset("../outside.bin", 1, new string('0', 32)))
                .Write(Path.Combine(root, OriginalContent.AssetPackManifestFileName));

            var issue = Assert.Single((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).Issues);
            Assert.Equal(InstalledAssetProblem.UnsafePath, issue.Problem);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task EveryPackFileRecordsItsMediaTypeAndConversion()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var assetPath = Path.Combine(root, "synthetic.bin");
            await File.WriteAllBytesAsync(assetPath, [1], TestContext.Current.CancellationToken);
            var asset = Asset("synthetic.bin", 1, await HashAsync(assetPath)) with { Conversion = null };
            Manifest(OriginalContent.RequiredAssetPackRevision, 'a', asset)
                .Write(Path.Combine(root, OriginalContent.AssetPackManifestFileName));

            var issue = Assert.Single((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).Issues);
            Assert.Equal(InstalledAssetProblem.InvalidRecord, issue.Problem);
            Assert.Equal("synthetic.bin", issue.Path);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task MissingAssetPackIsActionable()
    {
        var verification = await OriginalContent.VerifyInstalledAsync(
            Path.Combine(TestRoot(), "missing"), TestContext.Current.CancellationToken);

        var issue = Assert.Single(verification.Issues);
        Assert.Equal(InstalledAssetProblem.ManifestMissing, issue.Problem);
        Assert.Contains("DarkSunWakeRedux.Extractor", issue.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CorpusScaleManifestLimitRejectsOnlyOversizedInput()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var assetPath = Path.Combine(root, "synthetic.bin");
            await File.WriteAllBytesAsync(assetPath, [1], TestContext.Current.CancellationToken);
            var manifestPath = Path.Combine(root, OriginalContent.AssetPackManifestFileName);
            Manifest(OriginalContent.RequiredAssetPackRevision, 'a', Asset("synthetic.bin", 1, await HashAsync(assetPath)))
                .Write(manifestPath);
            // The full corpus manifest is larger than the toolkit's 4 MiB default; whitespace padding
            // stands in for its records.
            await File.AppendAllTextAsync(manifestPath,
                new string(' ', InstalledAssetManifest.DefaultMaximumBytes + 1024), TestContext.Current.CancellationToken);
            Assert.True((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).IsValid);

            await using (var stream = File.Create(manifestPath))
                stream.SetLength(OriginalContent.MaximumManifestBytes + 1);
            var issue = Assert.Single((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).Issues);
            Assert.Equal(InstalledAssetProblem.ManifestUnreadable, issue.Problem);
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
            Manifest(OriginalContent.RequiredAssetPackRevision - 1, 'c',
                    Asset("images/synthetic.bin", 1, await HashAsync(assetPath)))
                .Write(Path.Combine(root, OriginalContent.AssetPackManifestFileName));

            var issue = Assert.Single((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).Issues);

            Assert.Equal(InstalledAssetProblem.FormatVersionMismatch, issue.Problem);
            Assert.Contains(OriginalContent.RequiredAssetPackRevision.ToString(), issue.Detail, StringComparison.Ordinal);
            Assert.Contains((OriginalContent.RequiredAssetPackRevision - 1).ToString(), issue.Detail, StringComparison.Ordinal);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task PackOfAnotherGameIsRejected()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var assetPath = Path.Combine(root, "synthetic.bin");
            await File.WriteAllBytesAsync(assetPath, [1], TestContext.Current.CancellationToken);
            (Manifest(OriginalContent.RequiredAssetPackRevision, 'd', Asset("synthetic.bin", 1, await HashAsync(assetPath)))
                    with { Product = "other-game" })
                .Write(Path.Combine(root, OriginalContent.AssetPackManifestFileName));

            Assert.Equal(InstalledAssetProblem.ProductMismatch, Assert.Single((await OriginalContent.VerifyInstalledAsync(
                root, TestContext.Current.CancellationToken)).Issues).Problem);
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
                return Manifest(OriginalContent.RequiredAssetPackRevision, 'b',
                    Asset("images/synthetic.bin", 2, await HashAsync(asset)));
            });

            Assert.False(File.Exists(Path.Combine(output, "stale.bin")));
            Assert.True((await OriginalContent.VerifyInstalledAsync(
                output, TestContext.Current.CancellationToken)).IsValid);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task FailedStagedPackLeavesTheInstalledPackInPlace()
    {
        var root = TestRoot();
        var output = Path.Combine(root, "pack");
        Directory.CreateDirectory(output);
        await File.WriteAllTextAsync(Path.Combine(output, "previous.bin"), "previous",
            TestContext.Current.CancellationToken);
        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(() => AssetPackInstaller.InstallAsync(output, staging =>
                Task.FromResult(Manifest(OriginalContent.RequiredAssetPackRevision, 'b',
                    Asset("images/never-written.bin", 2, new string('0', 32))))));

            Assert.True(File.Exists(Path.Combine(output, "previous.bin")));
            Assert.Single(Directory.EnumerateDirectories(root));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static InstalledAssetManifest Manifest(int revision, char fingerprint, params InstalledAsset[] files) =>
        new(revision, OriginalContent.GameId, "synthetic-edition", new string(fingerprint, 32),
            DateTimeOffset.UnixEpoch, files, "test");

    private static InstalledAsset Asset(string path, long bytes, string xxh3) =>
        new(path, bytes, xxh3, "SOURCE.GFF", "application/octet-stream", new AssetConversion("synthetic-test"));

    private static string TestRoot() => Path.Combine(
        Path.GetTempPath(), "dark-sun-wake-redux-tests", Guid.NewGuid().ToString("N"));

    private static async Task<string> HashAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        return await FileFingerprint.Xxh3Async(stream, TestContext.Current.CancellationToken);
    }
}
