using System.Security.Cryptography;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Extractor;

public static class StartupAssetExtractor
{
    public const string SourcePath = "RESOURCE.GFF";
    public const string CharacterSourcePath = "CHARSAVE.GFF";
    public const string TitleImageTag = "BMP ";
    public const uint TitleImageNumber = 11011;
    public const string ImageTag = "ICON";
    public const string PaletteTag = "PAL ";
    public const uint TitlePaletteNumber = 11011;
    public const uint InterfacePaletteNumber = 1000;
    public const string FontTag = "FONT";
    public const uint FontNumber = 100;
    public const uint PartyWindowImageNumber = 19004;

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
        var characterSourcePath = Path.Combine(sourceRoot, CharacterSourcePath);
        if (!File.Exists(characterSourcePath))
            throw new FileNotFoundException(
                $"Required character storage is missing: {CharacterSourcePath}.",
                characterSourcePath);
        await using var characterSource = File.OpenRead(characterSourcePath);
        var characterArchive = GffArchive.Read(characterSource, characterSourcePath);
        var characters = GffCharacterCatalog.Read(characterArchive, CharacterSourcePath);
        var titlePalette = IndexedPalette.Read(
            archive.GetResource(PaletteTag, TitlePaletteNumber).Span,
            $"{SourcePath}:{PaletteTag}#{TitlePaletteNumber}");
        var interfacePalette = IndexedPalette.Read(
            archive.GetResource(PaletteTag, InterfacePaletteNumber).Span,
            $"{SourcePath}:{PaletteTag}#{InterfacePaletteNumber}");
        var files = new List<AssetPackFile>();

        var title = IndexedImage.Read(archive.GetResource(TitleImageTag, TitleImageNumber),
            $"{SourcePath}:{TitleImageTag}#{TitleImageNumber}");
        if (title.Frames.Count != 1 || title.Frames[0].Width != 320 || title.Frames[0].Height != 200)
            throw new InvalidDataException(
                $"The mapped title image must contain exactly one 320x200 frame; found {title.Frames.Count} frame(s).");
        files.Add(await WriteImageAsync(stagingRoot, OriginalContent.TitleImageAssetPath, title, titlePalette,
            $"{TitleImageTag}#{TitleImageNumber} frame 0 + {PaletteTag}#{TitlePaletteNumber}", cancellationToken));

        foreach (var layer in OriginalContent.StartMenuLayers)
        {
            var image = IndexedImage.Read(
                archive.GetResource(TitleImageTag, layer.ImageResourceNumber),
                $"{SourcePath}:{TitleImageTag}#{layer.ImageResourceNumber}");
            if (image.Frames.Count != 1 || image.Frames[0].Width != layer.FrameWidth ||
                image.Frames[0].Height != layer.FrameHeight)
                throw new InvalidDataException(
                    $"The mapped start-menu {layer.Name} image has unexpected frame geometry.");
            files.Add(await WriteImageAsync(stagingRoot, layer.Path, image, interfacePalette,
                $"{TitleImageTag}#{layer.ImageResourceNumber} frame 0 + " +
                $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));
        }

        foreach (var mapping in OriginalContent.StartMenuButtons)
        {
            var image = IndexedImage.Read(archive.GetResource(ImageTag, mapping.ImageResourceNumber),
                $"{SourcePath}:{ImageTag}#{mapping.ImageResourceNumber}");
            if (!mapping.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped {mapping.Name} image does not match its four-frame geometry contract.");
            files.Add(await WriteImageAsync(stagingRoot, mapping.Path, image, interfacePalette,
                $"{ImageTag}#{mapping.ImageResourceNumber} all frames + " +
                $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));
        }

        foreach (var mapping in OriginalContent.CharacterGenerationButtons)
        {
            var image = IndexedImage.Read(archive.GetResource(ImageTag, mapping.ImageResourceNumber),
                $"{SourcePath}:{ImageTag}#{mapping.ImageResourceNumber}");
            if (!mapping.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped character-generation {mapping.Name} image has unexpected frame geometry.");
            files.Add(await WriteImageAsync(stagingRoot, mapping.Path, image, interfacePalette,
                $"{ImageTag}#{mapping.ImageResourceNumber} all frames + " +
                $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));
        }
        foreach (var mapping in OriginalContent.CharacterGenerationModalButtons)
        {
            var image = IndexedImage.Read(archive.GetResource(ImageTag, mapping.ImageResourceNumber),
                $"{SourcePath}:{ImageTag}#{mapping.ImageResourceNumber}");
            if (!mapping.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped character-generation modal {mapping.Name} image has unexpected frame geometry.");
            files.Add(await WriteImageAsync(stagingRoot, mapping.Path, image, interfacePalette,
                $"{ImageTag}#{mapping.ImageResourceNumber} all frames + " +
                $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));
        }

        var windowImage = IndexedImage.Read(archive.GetResource(TitleImageTag, PartyWindowImageNumber),
            $"{SourcePath}:{TitleImageTag}#{PartyWindowImageNumber}");
        if (windowImage.Frames.Count != 1 || windowImage.Frames[0].Width != 96 ||
            windowImage.Frames[0].Height != 9)
            throw new InvalidDataException("The mapped party-window image must contain one 96x9 frame.");
        files.Add(await WriteImageAsync(stagingRoot, OriginalContent.PartyWindowImageAssetPath,
            windowImage, interfacePalette, $"{TitleImageTag}#{PartyWindowImageNumber} frame 0 + " +
            $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));

        var font = IndexedBitmapFont.Read(archive.GetResource(FontTag, FontNumber),
            $"{SourcePath}:{FontTag}#{FontNumber}");
        files.Add(await WriteFontAsync(stagingRoot, font, cancellationToken));
        files.Add(await WriteTextCatalogAsync(stagingRoot, archive, cancellationToken));
        files.Add(await WriteUiCatalogAsync(stagingRoot, archive, cancellationToken));
        files.Add(await WriteCharacterCatalogAsync(stagingRoot, characters, cancellationToken));

        return new AssetPackManifest(
            OriginalContent.AssetPackFormatVersion,
            OriginalContent.GameId,
            edition.SourceEdition,
            edition.Fingerprint(),
            extractorVersion,
            files);
    }

    private static async Task<AssetPackFile> WriteCharacterCatalogAsync(
        string stagingRoot,
        IReadOnlyList<GffCharacterCatalogEntry> characters,
        CancellationToken cancellationToken)
    {
        var catalog = PackedCharacterCatalog.From(characters);
        var relativePath = OriginalContent.CharacterCatalogAssetPath;
        var target = Path.Combine(stagingRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) catalog.Write(output);
        await using var verify = File.OpenRead(target);
        PackedCharacterCatalog.Read(verify, relativePath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(
            await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, CharacterSourcePath,
            "application/vnd.dark-sun-wake-redux.character-catalog",
            $"CHAR identity/abilities/envelope + PSIN mask -> DSCH v{PackedCharacterCatalog.FormatVersion}");
    }

    private static async Task<AssetPackFile> WriteUiCatalogAsync(
        string stagingRoot,
        GffArchive archive,
        CancellationToken cancellationToken)
    {
        var windows = OriginalContent.StartFlowWindowResourceNumbers.Select(number =>
            UiWindowResource.Read(archive.GetResource("WIND", number),
                $"{SourcePath}:WIND#{number}")).ToArray();
        if (!windows.Select(window => window.ResourceNumber)
            .SequenceEqual(OriginalContent.StartFlowWindowResourceNumbers))
            throw new InvalidDataException("A start-flow WIND record contains the wrong embedded resource number.");
        var children = windows.SelectMany(window => window.Children).ToArray();
        var unsupported = children.Select(child => child.Tag)
            .Distinct(StringComparer.Ordinal)
            .Where(tag => tag is not ("BUTN" or "APFM" or "EBOX"))
            .ToArray();
        if (unsupported.Length != 0)
            throw new InvalidDataException(
                $"Start-flow windows contain unsupported child tags: {string.Join(", ", unsupported)}.");
        var buttonNumbers = children.Where(child => child.Tag == "BUTN")
            .Select(child => child.ResourceNumber).Distinct().ToArray();
        var buttons = buttonNumbers.Select(number =>
                UiButtonResource.Read(archive.GetResource("BUTN", number),
                    $"{SourcePath}:BUTN#{number}")).ToArray();
        var frameNumbers = children.Where(child => child.Tag == "APFM")
            .Select(child => child.ResourceNumber).Distinct().ToArray();
        var frames = frameNumbers.Select(number =>
                UiApplicationFrameResource.Read(archive.GetResource("APFM", number),
                    $"{SourcePath}:APFM#{number}")).ToArray();
        var editBoxNumbers = children.Where(child => child.Tag == "EBOX")
            .Select(child => child.ResourceNumber).Distinct().ToArray();
        var editBoxes = editBoxNumbers.Select(number =>
                UiEditBoxResource.Read(archive.GetResource("EBOX", number),
                    $"{SourcePath}:EBOX#{number}")).ToArray();
        if (!buttons.Select(item => item.ResourceNumber).SequenceEqual(buttonNumbers) ||
            !frames.Select(item => item.ResourceNumber).SequenceEqual(frameNumbers) ||
            !editBoxes.Select(item => item.ResourceNumber).SequenceEqual(editBoxNumbers))
            throw new InvalidDataException("A start-flow child record contains the wrong embedded resource number.");
        var catalog = new PackedUiCatalog(windows, buttons, frames, editBoxes);
        var relativePath = OriginalContent.StartFlowUiCatalogAssetPath;
        var target = Path.Combine(stagingRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) catalog.Write(output);
        await using var verify = File.OpenRead(target);
        PackedUiCatalog.Read(verify, relativePath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, SourcePath,
            "application/vnd.dark-sun-wake-redux.ui-catalog",
            $"WIND#19500-19505 resolved child graph -> DSUI v{PackedUiCatalog.FormatVersion}");
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
