using System.Security.Cryptography;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Extractor;

public static class TitleAssetExtractor
{
    public const string SourcePath = "RESOURCE.GFF";
    public const string ImageTag = "BMP ";
    public const uint ImageNumber = 11011;
    public const string PaletteTag = "PAL ";
    public const uint PaletteNumber = 11011;

    public static async Task<AssetPackManifest> WritePackAsync(
        string sourceRoot,
        string stagingRoot,
        SourceManifest edition,
        string extractorVersion,
        CancellationToken cancellationToken = default)
    {
        var sourcePath = Path.Combine(sourceRoot, SourcePath);
        await using var source = File.OpenRead(sourcePath);
        var archive = GffArchive.Read(source, sourcePath);
        var image = IndexedImage.Read(archive.GetResource(ImageTag, ImageNumber),
            $"{SourcePath}:{ImageTag}#{ImageNumber}");
        var palette = IndexedPalette.Read(archive.GetResource(PaletteTag, PaletteNumber).Span,
            $"{SourcePath}:{PaletteTag}#{PaletteNumber}");
        if (image.Frames.Count != 1 || image.Frames[0].Width != 320 || image.Frames[0].Height != 200)
            throw new InvalidDataException(
                $"The mapped title image must contain exactly one 320x200 frame; found {image.Frames.Count} frame(s).");

        var target = Path.Combine(stagingRoot,
            OriginalContent.TitleImageAssetPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target))
            PackedIndexedImage.From(image, palette).Write(output);
        await using var verify = File.OpenRead(target);
        PackedIndexedImage.Read(verify, OriginalContent.TitleImageAssetPath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(verify, cancellationToken));
        var size = verify.Length;
        return new AssetPackManifest(
            OriginalContent.AssetPackFormatVersion,
            OriginalContent.GameId,
            edition.SourceEdition,
            edition.Fingerprint(),
            extractorVersion,
            [new AssetPackFile(
                OriginalContent.TitleImageAssetPath,
                size,
                hash,
                SourcePath,
                "application/vnd.dark-sun-wake-redux.indexed-image",
                $"{ImageTag}#{ImageNumber} frame 0 + {PaletteTag}#{PaletteNumber} -> DSIX v{PackedIndexedImage.FormatVersion}")]);
    }
}
