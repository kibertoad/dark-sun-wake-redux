using RefurbishedDinosaurs.Core.Assets;
using RefurbishedDinosaurs.Core.IO;

namespace DarkSunWakeRedux.Extractor;

public enum SourceCorpusDisposition
{
    ImmutableGameData,
    MutableCaptureOrSave,
    DosBoxWrapper,
    StorefrontWrapper,
    Documentation,
    Unrepresented
}

public sealed record SourceCorpusInventoryRecord(string Path, SourceCorpusDisposition Disposition);

public sealed record SourceCorpusInventory(IReadOnlyList<SourceCorpusInventoryRecord> Records)
{
    public bool IsComplete => Records.All(record =>
        record.Disposition != SourceCorpusDisposition.Unrepresented);

    public static SourceCorpusInventory Read(string root, AssetManifest manifest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(manifest);
        manifest.Validate();
        var fullRoot = Path.GetFullPath(root).TrimEnd(
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var included = manifest.Files.Select(file => PortableAssetPath.Relative(file.Path))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var records = Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(fullRoot, path).Replace('\\', '/'))
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(path => new SourceCorpusInventoryRecord(path,
                included.Contains(path) ? SourceCorpusDisposition.ImmutableGameData : ClassifyExcluded(path)))
            .ToArray();
        return new(records);
    }

    private static SourceCorpusDisposition ClassifyExcluded(string path)
    {
        if (path.StartsWith("capture/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("cloud_saves/", StringComparison.OrdinalIgnoreCase))
            return SourceCorpusDisposition.MutableCaptureOrSave;
        if (path.StartsWith("DOSBOX/", StringComparison.OrdinalIgnoreCase))
            return SourceCorpusDisposition.DosBoxWrapper;
        if (path.Equals("dosbox_darksun2.conf", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("dosbox_darksun2_single.conf", StringComparison.OrdinalIgnoreCase))
            return SourceCorpusDisposition.DosBoxWrapper;
        if (path.Equals("Manual.pdf", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("ds_wakerave_manual_pdf.pdf", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("Cluebook.pdf", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("EULA.txt", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("README.TXT", StringComparison.OrdinalIgnoreCase))
            return SourceCorpusDisposition.Documentation;
        if (path.StartsWith("unins000", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("goggame-", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("goglog.ini", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("gog.ico", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("support.ico", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("webcache.zip", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("Launch Dark Sun 2 - Wake of the Ravager.lnk",
                StringComparison.OrdinalIgnoreCase))
            return SourceCorpusDisposition.StorefrontWrapper;
        return SourceCorpusDisposition.Unrepresented;
    }
}
