using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Extractor;

public static class RegionCatalogExtractor
{
    public static IReadOnlyList<string> SourcePaths(SourceManifest edition)
    {
        ArgumentNullException.ThrowIfNull(edition);
        edition.Validate();
        return edition.Files
            .Select(file => SourceManifest.Normalize(file.Path))
            .Where(IsRegionSourcePath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static async Task<IReadOnlyList<AssetPackFile>> WriteAsync(
        string sourceRoot,
        string stagingRoot,
        SourceManifest edition,
        GffArchive objectArchive,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(stagingRoot);
        ArgumentNullException.ThrowIfNull(objectArchive);

        var sourcePaths = SourcePaths(edition);
        if (sourcePaths.Count == 0)
            throw new InvalidDataException(
                "The supported source manifest contains no RGNxxx.GFF region archives.");
        var outputPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var files = new List<AssetPackFile>(sourcePaths.Count);
        foreach (var sourcePath in sourcePaths)
        {
            var relativePath = OriginalContent.RegionCatalogAssetPathFor(sourcePath);
            if (!outputPaths.Add(relativePath))
                throw new InvalidDataException(
                    $"Region catalog output path is duplicated: {relativePath}.");
            var inputPath = Path.Combine(sourceRoot,
                sourcePath.Replace('/', Path.DirectorySeparatorChar));
            await using var input = File.OpenRead(inputPath);
            var archive = GffArchive.Read(input, sourcePath);
            var region = GffRegion.Read(archive, objectArchive, sourcePath);
            files.Add(await WriteRegionAsync(stagingRoot, relativePath, sourcePath,
                region, cancellationToken));
        }
        return files;
    }

    private static bool IsRegionSourcePath(string sourcePath)
    {
        if (sourcePath.Contains('/') || sourcePath.Length != 10 ||
            !sourcePath.EndsWith(".GFF", StringComparison.OrdinalIgnoreCase) ||
            !sourcePath.StartsWith("RGN", StringComparison.OrdinalIgnoreCase))
            return false;
        return sourcePath.AsSpan(3, 3).ToString().All(Uri.IsHexDigit);
    }

    private static async Task<AssetPackFile> WriteRegionAsync(
        string stagingRoot,
        string relativePath,
        string sourcePath,
        GffRegion region,
        CancellationToken cancellationToken)
    {
        var packed = PackedRegion.From(region);
        var target = Path.Combine(stagingRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) packed.Write(output);
        await using var verify = File.OpenRead(target);
        var decoded = PackedRegion.Read(verify, relativePath);
        if (decoded.ResourceNumber != region.ResourceNumber || decoded.Name != region.Name ||
            !decoded.TileMap.SequenceEqual(region.TileMap) ||
            !decoded.GeometryMap.SequenceEqual(region.GeometryMap) ||
            decoded.Tiles.Count != region.Tiles.Count || decoded.Entities.Count != region.Entities.Count)
            throw new InvalidDataException(
                $"{sourcePath}: the derived structural region catalog failed verification.");
        verify.Position = 0;
        var hash = await ContentHash.Xxh3Async(verify, cancellationToken);
        return new(relativePath, verify.Length, hash, sourcePath,
            "application/vnd.dark-sun-wake-redux.region",
            "RNME/PAL/MAP/GMAP/TILE/ETAB + OBJEX.GFF:OJFF references -> " +
            $"source-derived structural DSRG v{PackedRegion.FormatVersion}");
    }
}
