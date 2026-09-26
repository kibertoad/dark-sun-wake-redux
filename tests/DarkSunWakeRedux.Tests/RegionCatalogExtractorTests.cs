using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class RegionCatalogExtractorTests
{
    [Fact]
    public async Task WritesEveryManifestRegionInCanonicalSourcePathOrder()
    {
        var root = TestRoot();
        var sourceRoot = Path.Combine(root, "source");
        var stagingRoot = Path.Combine(root, "staging");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(stagingRoot);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(sourceRoot, "RGN033.GFF"),
                GffRegionTests.RegionArchive(), TestContext.Current.CancellationToken);
            await File.WriteAllBytesAsync(Path.Combine(sourceRoot, "RGN001.GFF"),
                GffRegionTests.RegionArchive(), TestContext.Current.CancellationToken);
            var edition = new SourceManifest("game", "synthetic",
            [
                await FingerprintAsync(sourceRoot, "RGN033.GFF"),
                await FingerprintAsync(sourceRoot, "RGN001.GFF")
            ]);
            using var objectStream = new MemoryStream(GffRegionTests.ObjectArchive());
            var objects = GffArchive.Read(objectStream, "OBJEX.GFF");

            var files = await RegionCatalogExtractor.WriteAsync(sourceRoot, stagingRoot,
                edition, objects, TestContext.Current.CancellationToken);

            Assert.Equal(["RGN001.GFF", "RGN033.GFF"],
                files.Select(file => file.SourcePath));
            Assert.Equal(["regions/structural/rgn001.dsrg", "regions/structural/rgn033.dsrg"],
                files.Select(file => file.Path));
            foreach (var file in files)
            {
                await using var stream = File.OpenRead(Path.Combine(stagingRoot,
                    file.Path.Replace('/', Path.DirectorySeparatorChar)));
                var region = PackedRegion.Read(stream, file.Path);
                Assert.Equal((50U, "Tyr"),
                    (region.ResourceNumber, region.Name));
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ReportsTheMalformedNonTyrRegionSourcePath()
    {
        var root = TestRoot();
        var sourceRoot = Path.Combine(root, "source");
        var stagingRoot = Path.Combine(root, "staging");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(stagingRoot);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(sourceRoot, "RGN033.GFF"),
                GffRegionTests.RegionArchive(mapLength: GffRegion.MapByteCount - 1),
                TestContext.Current.CancellationToken);
            var edition = new SourceManifest("game", "synthetic",
                [await FingerprintAsync(sourceRoot, "RGN033.GFF")]);
            using var objectStream = new MemoryStream(GffRegionTests.ObjectArchive());
            var objects = GffArchive.Read(objectStream, "OBJEX.GFF");

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                RegionCatalogExtractor.WriteAsync(sourceRoot, stagingRoot, edition, objects,
                    TestContext.Current.CancellationToken));

            Assert.Contains("RGN033.GFF: MAP #50 must contain exactly 12544 bytes", exception.Message);
            Assert.Empty(Directory.EnumerateFileSystemEntries(stagingRoot));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void RejectsDuplicateRegionSourcePathsBeforeDerivingOutputPaths()
    {
        var edition = new SourceManifest("game", "synthetic",
        [
            new("RGN001.GFF", 0, new string('0', 32)),
            new("rgn001.gff", 0, new string('1', 32))
        ]);

        var exception = Assert.Throws<InvalidDataException>(() =>
            RegionCatalogExtractor.SourcePaths(edition));

        Assert.Contains("Duplicate source path", exception.Message);
    }

    private static async Task<SourceFile> FingerprintAsync(string sourceRoot, string path)
    {
        var inputPath = Path.Combine(sourceRoot, path);
        await using var input = File.OpenRead(inputPath);
        return new(path, input.Length, await ContentHash.Xxh3Async(input, TestContext.Current.CancellationToken));
    }

    private static string TestRoot() => Path.Combine(Path.GetTempPath(),
        "dark-sun-region-catalog-tests", Guid.NewGuid().ToString("N"));
}
