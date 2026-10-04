using RefurbishedDinosaurs.Core.Assets;
using RefurbishedDinosaurs.LegacyFormats;
using Xunit;

namespace DarkSunWakeRedux.Tests;

/// <summary>
/// Source verification of a directory installation, the only source kind a supported edition of this
/// game declares. The readers themselves are RefurbishedDinosaurs.LegacyFormats'
/// <see cref="OriginalContentSource"/>, tested in that package; these tests cover how edition
/// manifests in the Extractor's form reach and judge them through <see cref="AssetVerifier"/>.
/// </summary>
public sealed class SourceVerificationTests
{
    [Fact]
    public async Task DirectorySourceMatchesManifestPathsIgnoringCase()
    {
        var root = TestRoot();
        Directory.CreateDirectory(Path.Combine(root, "Game"));
        try
        {
            var payload = new byte[] { 9, 8, 7 };
            await File.WriteAllBytesAsync(Path.Combine(root, "Game", "TEST.BIN"), payload,
                TestContext.Current.CancellationToken);

            // The manifest's default source kind is a directory.
            var manifest = new AssetManifest("game", "synthetic",
                [new("game/test.bin", payload.Length, FileFingerprint.Xxh3(payload))]);
            Assert.True((await AssetVerifier.VerifyAsync(root, manifest,
                TestContext.Current.CancellationToken)).IsValid);

            var missing = new AssetManifest("game", "synthetic",
                [new("Game/OTHER.BIN", 1, new string('0', 32))]);
            Assert.Equal(AssetProblem.Missing, Assert.Single((await AssetVerifier.VerifyAsync(
                root, missing, TestContext.Current.CancellationToken)).Issues).Problem);

            // Identification names the edition the copy is and why the others are not.
            var identification = await AssetVerifier.IdentifyAsync(root, [missing, manifest with { SourceEdition = "match" }],
                TestContext.Current.CancellationToken);
            Assert.Equal("match", identification.Edition?.SourceEdition);
            Assert.Equal(AssetProblem.Missing, Assert.Single(Assert.Single(identification.Mismatches).Issues).Problem);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task CopyThatTwoEditionsBothDescribeIsAmbiguous()
    {
        var root = TestRoot();
        Directory.CreateDirectory(root);
        try
        {
            var payload = new byte[] { 1, 2, 3 };
            await File.WriteAllBytesAsync(Path.Combine(root, "GAME.DAT"), payload,
                TestContext.Current.CancellationToken);
            var first = new AssetManifest("game", "first",
                [new("GAME.DAT", payload.Length, FileFingerprint.Xxh3(payload))]);

            var identification = await AssetVerifier.IdentifyAsync(root,
                [first, first with { SourceEdition = "second" }], TestContext.Current.CancellationToken);

            // Neither edition is chosen: the manifests need a file that tells them apart.
            Assert.True(identification.IsAmbiguous);
            Assert.False(identification.IsSupported);
            Assert.Null(identification.Edition);
            Assert.Equal(["first", "second"], identification.Matches.Select(edition => edition.SourceEdition));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void FingerprintIdentifiesContentRatherThanTransport()
    {
        AssetManifest Manifest(string kind) => new("game", "edition",
            [new("GAME/TEST.BIN", 4, new string('a', 32))], kind);

        Assert.Equal(Manifest(ContentSourceKinds.Directory).Fingerprint(),
            Manifest(ContentSourceKinds.Iso9660).Fingerprint());
        Assert.Equal(Manifest(ContentSourceKinds.Directory).Fingerprint(),
            Manifest(ContentSourceKinds.CueBin).Fingerprint());
    }

    [Fact]
    public async Task ManifestWithAnUnknownSourceKindIsRejected()
    {
        var manifest = new AssetManifest("game", "edition", [new("file", 0, new string('0', 32))], "zip");
        await Assert.ThrowsAsync<InvalidDataException>(() => AssetVerifier.VerifyAsync(
            Path.GetTempPath(), manifest, TestContext.Current.CancellationToken));
    }

    private static string TestRoot() => Path.Combine(Path.GetTempPath(),
        "dark-sun-wake-redux-source-tests", Guid.NewGuid().ToString("N"));
}
