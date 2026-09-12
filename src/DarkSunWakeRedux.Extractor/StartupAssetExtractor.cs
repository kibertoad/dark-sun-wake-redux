using System.Security.Cryptography;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Extractor;

public static class StartupAssetExtractor
{
    public const string SourcePath = "RESOURCE.GFF";
    public const string TitleImageTag = "BMP ";
    public const uint TitleImageNumber = 11011;
    public const string ImageTag = "ICON";
    public const string PaletteTag = "PAL ";
    public const uint PaletteNumber = 11011;
    public const string FontTag = "FONT";
    public const uint FontNumber = 100;

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
        var palette = IndexedPalette.Read(archive.GetResource(PaletteTag, PaletteNumber).Span,
            $"{SourcePath}:{PaletteTag}#{PaletteNumber}");
        var files = new List<AssetPackFile>();

        var title = IndexedImage.Read(archive.GetResource(TitleImageTag, TitleImageNumber),
            $"{SourcePath}:{TitleImageTag}#{TitleImageNumber}");
        if (title.Frames.Count != 1 || title.Frames[0].Width != 320 || title.Frames[0].Height != 200)
            throw new InvalidDataException(
                $"The mapped title image must contain exactly one 320x200 frame; found {title.Frames.Count} frame(s).");
        files.Add(await WriteImageAsync(stagingRoot, OriginalContent.TitleImageAssetPath, title, palette,
            $"{TitleImageTag}#{TitleImageNumber} frame 0 + {PaletteTag}#{PaletteNumber}", cancellationToken));

        foreach (var mapping in OriginalContent.StartMenuButtons)
        {
            var image = IndexedImage.Read(archive.GetResource(ImageTag, mapping.ImageResourceNumber),
                $"{SourcePath}:{ImageTag}#{mapping.ImageResourceNumber}");
            if (!mapping.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped {mapping.Name} image does not match its four-frame geometry contract.");
            files.Add(await WriteImageAsync(stagingRoot, mapping.Path, image, palette,
                $"{ImageTag}#{mapping.ImageResourceNumber} all frames + {PaletteTag}#{PaletteNumber}", cancellationToken));
        }

        var font = IndexedBitmapFont.Read(archive.GetResource(FontTag, FontNumber),
            $"{SourcePath}:{FontTag}#{FontNumber}");
        files.Add(await WriteFontAsync(stagingRoot, font, cancellationToken));
        files.Add(await WriteTextCatalogAsync(stagingRoot, archive, cancellationToken));

        return new AssetPackManifest(
            OriginalContent.AssetPackFormatVersion,
            OriginalContent.GameId,
            edition.SourceEdition,
            edition.Fingerprint(),
            extractorVersion,
            files);
    }

    private static async Task<AssetPackFile> WriteTextCatalogAsync(
        string stagingRoot,
        GffArchive archive,
        CancellationToken cancellationToken)
    {
        var resources = archive.Resources.Where(resource => resource.Tag == "TEXT")
            .OrderBy(resource => resource.Number)
            .ToDictionary(resource => resource.Number, resource =>
                (IReadOnlyList<string>)GffTextResource.Read(
                    archive.GetResource(resource.Tag, resource.Number),
                    $"{SourcePath}:{resource.Tag}#{resource.Number}").Lines.ToArray());
        var target = Path.Combine(stagingRoot,
            OriginalContent.TextCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) new PackedTextCatalog(resources).Write(output);
        await using var verify = File.OpenRead(target);
        PackedTextCatalog.Read(verify, OriginalContent.TextCatalogAssetPath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(verify, cancellationToken));
        return new(OriginalContent.TextCatalogAssetPath, verify.Length, hash, SourcePath,
            "application/vnd.dark-sun-wake-redux.text-catalog", "all TEXT resources -> DSTX v1");
    }

    private static async Task<AssetPackFile> WriteFontAsync(
        string stagingRoot,
        IndexedBitmapFont font,
        CancellationToken cancellationToken)
    {
        var relativePath = OriginalContent.InterfaceFontAssetPath;
        var target = Path.Combine(stagingRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target))
            PackedIndexedBitmapFont.From(font).Write(output);
        await using var verify = File.OpenRead(target);
        PackedIndexedBitmapFont.Read(verify, relativePath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, SourcePath,
            "application/vnd.dark-sun-wake-redux.indexed-font",
            $"{FontTag}#{FontNumber} -> DSFT v{PackedIndexedBitmapFont.FormatVersion}");
    }

    private static async Task<AssetPackFile> WriteImageAsync(
        string stagingRoot,
        string relativePath,
        IndexedImage image,
        IndexedPalette palette,
        string sourceMapping,
        CancellationToken cancellationToken)
    {
        var target = Path.Combine(stagingRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target))
            PackedIndexedImage.From(image, palette).Write(output);
        await using var verify = File.OpenRead(target);
        PackedIndexedImage.Read(verify, relativePath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, SourcePath,
            "application/vnd.dark-sun-wake-redux.indexed-image",
            $"{sourceMapping} -> DSIX v{PackedIndexedImage.FormatVersion}");
    }
}
