using RefurbishedDinosaurs.Core.Assets;

namespace DarkSunWakeRedux.Extractor;

internal static class PackAsset
{
    /// <summary>
    /// One pack file's manifest record. <paramref name="conversion"/> names the source resources and
    /// the pack format they were written as, and becomes the record's conversion method.
    /// </summary>
    public static InstalledAsset Create(
        string path,
        long bytes,
        string xxh3,
        string sourcePath,
        string mediaType,
        string conversion) =>
        new(path, bytes, xxh3, sourcePath, mediaType, new AssetConversion(conversion));
}
