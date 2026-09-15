using System.Security.Cryptography;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Extractor;

public static class StartupAssetExtractor
{
    public const string SourcePath = "RESOURCE.GFF";
    public const string ExecutableSourcePath = "DSUN.EXE";
    public const string CharacterSourcePath = "CHARSAVE.GFF";
    public const string GplSourcePath = "GPLDATA.GFF";
    public const string ObjectSourcePath = "OBJEX.GFF";
    public const string TyrRegionSourcePath = "RGN032.GFF";
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
        var executablePath = Path.Combine(sourceRoot, ExecutableSourcePath);
        if (!File.Exists(executablePath))
            throw new FileNotFoundException(
                $"Required game executable is missing: {ExecutableSourcePath}.",
                executablePath);
        await using var executable = File.OpenRead(executablePath);
        var preferencesText = ExecutablePreferencesTextReader.Read(
            executable, ExecutableSourcePath);
        var characterSourcePath = Path.Combine(sourceRoot, CharacterSourcePath);
        if (!File.Exists(characterSourcePath))
            throw new FileNotFoundException(
                $"Required character storage is missing: {CharacterSourcePath}.",
                characterSourcePath);
        await using var characterSource = File.OpenRead(characterSourcePath);
        var characterArchive = GffArchive.Read(characterSource, characterSourcePath);
        var characters = GffCharacterCatalog.Read(characterArchive, CharacterSourcePath);
        var gplSourcePath = Path.Combine(sourceRoot, GplSourcePath);
        await using var gplSource = File.OpenRead(gplSourcePath);
        var gplArchive = GffArchive.Read(gplSource, gplSourcePath);
        var objectSourcePath = Path.Combine(sourceRoot, ObjectSourcePath);
        await using var objectSource = File.OpenRead(objectSourcePath);
        var objectArchive = GffArchive.Read(objectSource, objectSourcePath);
        var regionSourcePath = Path.Combine(sourceRoot, TyrRegionSourcePath);
        await using var regionSource = File.OpenRead(regionSourcePath);
        var regionArchive = GffArchive.Read(regionSource, regionSourcePath);
        var tyrRegion = GffRegion.Read(regionArchive, objectArchive, TyrRegionSourcePath);
        if (tyrRegion.ResourceNumber != 50 || tyrRegion.Name != "Tyr")
            throw new InvalidDataException(
                $"{TyrRegionSourcePath} does not contain the expected Tyr region #50.");
        var tyrObjects = GffObjectFrameCatalog.Read(objectArchive,
            tyrRegion.Entities.Select(entity => entity.ObjectResourceNumber), ObjectSourcePath);
        var openingLeader = GffObjectFrameCatalog.Read(objectArchive,
            [OriginalContent.OpeningLeaderObjectResourceNumber], ObjectSourcePath).Entries.Single();
        if (openingLeader.ImageResourceNumber != OriginalContent.OpeningLeaderImageResourceNumber)
            throw new InvalidDataException(
                $"{ObjectSourcePath}:OJFF #{OriginalContent.OpeningLeaderObjectResourceNumber} " +
                $"must reference BMP #{OriginalContent.OpeningLeaderImageResourceNumber}.");
        var openingLeaderFrame = openingLeader.Image.Frames[0];
        if (openingLeaderFrame.Width != OpeningTyrScene.LeaderWidth ||
            openingLeaderFrame.Height != OpeningTyrScene.LeaderHeight)
            throw new InvalidDataException(
                $"The mapped opening leader frame must be {OpeningTyrScene.LeaderWidth}x" +
                $"{OpeningTyrScene.LeaderHeight} pixels.");
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
            files.Add(await ExtractLayerAsync(archive, interfacePalette, stagingRoot,
                layer, "start-menu", cancellationToken));
        foreach (var layer in OriginalContent.PartyOverviewLayers)
            files.Add(await ExtractLayerAsync(archive, interfacePalette, stagingRoot,
                layer, "party-overview", cancellationToken));
        foreach (var asset in OriginalContent.AddExistingCharacterAssets)
        {
            var image = IndexedImage.Read(archive.GetResource(asset.Tag, asset.ResourceNumber),
                $"{SourcePath}:{asset.Tag}#{asset.ResourceNumber}");
            if (!asset.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped add-existing {asset.Name} image has unexpected frame geometry.");
            files.Add(await WriteImageAsync(stagingRoot, asset.Path, image, interfacePalette,
                $"{asset.Tag}#{asset.ResourceNumber} all frames + " +
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
        files.Add(await ExtractLayerAsync(archive, interfacePalette, stagingRoot,
            OriginalContent.GameMenuLayer, "game-menu", cancellationToken));
        files.Add(await ExtractLayerAsync(archive, interfacePalette, stagingRoot,
            OriginalContent.InventoryLayer, "inventory", cancellationToken));
        foreach (var layer in OriginalContent.ExplorationDestinationTitleLayers)
            files.Add(await ExtractLayerAsync(archive, interfacePalette, stagingRoot,
                layer, "exploration-destination", cancellationToken));
        foreach (var mapping in OriginalContent.GameMenuButtons
                     .Concat(OriginalContent.PreferencesButtons)
                     .DistinctBy(asset => asset.Path))
        {
            var image = IndexedImage.Read(archive.GetResource(ImageTag, mapping.ImageResourceNumber),
                $"{SourcePath}:{ImageTag}#{mapping.ImageResourceNumber}");
            if (!mapping.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped game-menu {mapping.Name} image has unexpected frame geometry.");
            files.Add(await WriteImageAsync(stagingRoot, mapping.Path, image, interfacePalette,
                $"{ImageTag}#{mapping.ImageResourceNumber} all frames + " +
                $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));
        }
        foreach (var asset in OriginalContent.ExplorationCursorAssets)
        {
            var image = IndexedImage.Read(archive.GetResource(asset.Tag, asset.ResourceNumber),
                $"{SourcePath}:{asset.Tag}#{asset.ResourceNumber}");
            if (!asset.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped exploration cursor {asset.Name} has unexpected frame geometry.");
            files.Add(await WriteImageAsync(stagingRoot, asset.Path, image, interfacePalette,
                $"{asset.Tag}#{asset.ResourceNumber} all frames + " +
                $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));
        }
        foreach (var asset in OriginalContent.InteractionButtonAssets)
        {
            var image = IndexedImage.Read(
                archive.GetResource(ImageTag, asset.ImageResourceNumber),
                $"{SourcePath}:{ImageTag}#{asset.ImageResourceNumber}");
            if (!asset.HasExpectedFrames(image.Frames))
                throw new InvalidDataException(
                    $"The mapped interaction {asset.Name} image has unexpected frame geometry.");
            files.Add(await WriteImageAsync(stagingRoot, asset.Path, image, interfacePalette,
                $"{ImageTag}#{asset.ImageResourceNumber} all frames + " +
                $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken));
        }
        var dialoguePortrait = IndexedImage.Read(gplArchive.GetResource(
                "PORT", OriginalContent.FirstTyrDialoguePortraitResourceNumber),
            $"{GplSourcePath}:PORT#{OriginalContent.FirstTyrDialoguePortraitResourceNumber}");
        if (dialoguePortrait.Frames.Count != 1 ||
            dialoguePortrait.Frames[0].Width != 72 || dialoguePortrait.Frames[0].Height != 72)
            throw new InvalidDataException(
                "The mapped first Tyr dialogue portrait must contain one 72x72 frame.");
        files.Add(await WriteImageAsync(stagingRoot,
            OriginalContent.FirstTyrDialoguePortraitAssetPath, dialoguePortrait,
            interfacePalette,
            $"PORT#{OriginalContent.FirstTyrDialoguePortraitResourceNumber} frame 0 + " +
            $"{SourcePath}:{PaletteTag}#{InterfacePaletteNumber}", cancellationToken,
            GplSourcePath));

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
        files.Add(await WritePreferencesTextCatalogAsync(
            stagingRoot, preferencesText, cancellationToken));
        files.Add(await WriteUiCatalogAsync(stagingRoot, archive,
            OriginalContent.StartFlowUiCatalogAssetPath,
            OriginalContent.StartFlowWindowResourceNumbers, "start-flow", cancellationToken));
        files.Add(await WriteUiCatalogAsync(stagingRoot, archive,
            OriginalContent.GameMenuUiCatalogAssetPath,
            OriginalContent.GameMenuWindowResourceNumbers, "game-menu", cancellationToken));
        files.Add(await WriteUiCatalogAsync(stagingRoot, archive,
            OriginalContent.ExplorationDestinationUiCatalogAssetPath,
            OriginalContent.ExplorationDestinationWindowResourceNumbers,
            "exploration-destination", cancellationToken));
        files.Add(await WriteUiCatalogAsync(stagingRoot, archive,
            OriginalContent.InteractionUiCatalogAssetPath,
            OriginalContent.InteractionWindowResourceNumbers,
            "interaction", cancellationToken));
        files.Add(await WriteGplScriptAsync(stagingRoot, gplArchive,
            "GPL ", OriginalContent.FirstTyrDialogueScriptResourceNumber,
            OriginalContent.FirstTyrDialogueScriptAssetPath, cancellationToken));
        files.Add(await WriteGplScriptAsync(stagingRoot, gplArchive,
            "MAS ", OriginalContent.DialogueGlobalStringsScriptResourceNumber,
            OriginalContent.DialogueGlobalStringsScriptAssetPath, cancellationToken));
        files.Add(await WriteCharacterCatalogAsync(stagingRoot, characters, cancellationToken));
        files.Add(await WriteImageAsync(stagingRoot, OriginalContent.OpeningLeaderImageAssetPath,
            openingLeader.Image, tyrRegion.Palette,
            $"OJFF #{OriginalContent.OpeningLeaderObjectResourceNumber} -> " +
            $"BMP #{OriginalContent.OpeningLeaderImageResourceNumber} all frames + " +
            $"{TyrRegionSourcePath}:PAL #{tyrRegion.ResourceNumber}", cancellationToken,
            ObjectSourcePath));
        files.Add(await WriteRegionAsync(stagingRoot, tyrRegion, cancellationToken));
        files.Add(await WriteObjectCatalogAsync(stagingRoot, tyrObjects, cancellationToken));

        return new AssetPackManifest(
            OriginalContent.AssetPackFormatVersion,
            OriginalContent.GameId,
            edition.SourceEdition,
            edition.Fingerprint(),
            extractorVersion,
            files);
    }

    private static async Task<AssetPackFile> ExtractLayerAsync(
        GffArchive archive,
        IndexedPalette palette,
        string stagingRoot,
        UiLayerAsset layer,
        string family,
        CancellationToken cancellationToken)
    {
        var image = IndexedImage.Read(
            archive.GetResource(TitleImageTag, layer.ImageResourceNumber),
            $"{SourcePath}:{TitleImageTag}#{layer.ImageResourceNumber}");
        if (image.Frames.Count != 1 || image.Frames[0].Width != layer.FrameWidth ||
            image.Frames[0].Height != layer.FrameHeight)
            throw new InvalidDataException(
                $"The mapped {family} {layer.Name} image has unexpected frame geometry.");
        return await WriteImageAsync(stagingRoot, layer.Path, image, palette,
            $"{TitleImageTag}#{layer.ImageResourceNumber} frame 0 + " +
            $"{PaletteTag}#{InterfacePaletteNumber}", cancellationToken);
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

    private static async Task<AssetPackFile> WritePreferencesTextCatalogAsync(
        string stagingRoot,
        ExecutablePreferencesText text,
        CancellationToken cancellationToken)
    {
        var catalog = new PackedTextCatalog(new Dictionary<uint, IReadOnlyList<string>>
        {
            [OriginalContent.PreferencesDifficultyTextResourceNumber] =
                text.DifficultyLabels,
            [OriginalContent.PreferencesAboutTextResourceNumber] = text.AboutLines
        });
        var relativePath = OriginalContent.PreferencesTextCatalogAssetPath;
        var target = Path.Combine(stagingRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) catalog.Write(output);
        await using var verify = File.OpenRead(target);
        PackedTextCatalog.Read(verify, relativePath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(
            await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, ExecutableSourcePath,
            "application/vnd.dark-sun-wake-redux.text-catalog",
            $"bounded Preferences difficulty/About strings -> DSTX v{PackedTextCatalog.FormatVersion}");
    }

    private static async Task<AssetPackFile> WriteGplScriptAsync(
        string stagingRoot,
        GffArchive archive,
        string tag,
        uint number,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var packed = new PackedGplScript(number,
            archive.GetResource(tag, number).ToArray());
        var target = Path.Combine(stagingRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) packed.Write(output);
        await using var verify = File.OpenRead(target);
        var decoded = PackedGplScript.Read(verify, relativePath);
        if (decoded.ResourceNumber != number || !decoded.Bytecode.SequenceEqual(packed.Bytecode))
            throw new InvalidDataException(
                $"The derived {tag.Trim()} #{number} script failed verification.");
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(
            await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, GplSourcePath,
            "application/vnd.dark-sun-wake-redux.gpl-script",
            $"{tag.Trim()} #{number} bytecode -> DSGP v{PackedGplScript.FormatVersion}");
    }

    private static async Task<AssetPackFile> WriteRegionAsync(
        string stagingRoot,
        GffRegion region,
        CancellationToken cancellationToken)
    {
        var packed = PackedRegion.From(region);
        var relativePath = OriginalContent.TyrRegionAssetPath;
        var target = Path.Combine(stagingRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) packed.Write(output);
        await using var verify = File.OpenRead(target);
        var decoded = PackedRegion.Read(verify, relativePath);
        if (decoded.ResourceNumber != 50 || decoded.Name != "Tyr")
            throw new InvalidDataException("The derived Tyr region failed identity verification.");
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(
            await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, TyrRegionSourcePath,
            "application/vnd.dark-sun-wake-redux.region",
            $"RNME/PAL/MAP/GMAP/TILE/ETAB + {ObjectSourcePath}:OJFF references -> " +
            $"DSRG v{PackedRegion.FormatVersion}");
    }

    private static async Task<AssetPackFile> WriteObjectCatalogAsync(
        string stagingRoot,
        GffObjectFrameCatalog objects,
        CancellationToken cancellationToken)
    {
        var packed = PackedObjectFrameCatalog.From(objects);
        var relativePath = OriginalContent.TyrObjectCatalogAssetPath;
        var target = Path.Combine(stagingRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) packed.Write(output);
        await using var verify = File.OpenRead(target);
        var decoded = PackedObjectFrameCatalog.Read(verify, relativePath);
        if (decoded.Definitions.Count != objects.Entries.Count)
            throw new InvalidDataException("The derived Tyr object catalog failed count verification.");
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(
            await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, ObjectSourcePath,
            "application/vnd.dark-sun-wake-redux.object-frame-catalog",
            $"{TyrRegionSourcePath}:ETAB referenced {ObjectSourcePath}:OJFF/BMP -> " +
            $"DSOB v{PackedObjectFrameCatalog.FormatVersion}");
    }

    private static async Task<AssetPackFile> WriteUiCatalogAsync(
        string stagingRoot,
        GffArchive archive,
        string relativePath,
        IReadOnlyList<uint> windowResourceNumbers,
        string family,
        CancellationToken cancellationToken)
    {
        var windows = windowResourceNumbers.Select(number =>
            UiWindowResource.Read(archive.GetResource("WIND", number),
                $"{SourcePath}:WIND#{number}")).ToArray();
        if (!windows.Select(window => window.ResourceNumber)
            .SequenceEqual(windowResourceNumbers))
            throw new InvalidDataException(
                $"A {family} WIND record contains the wrong embedded resource number.");
        var children = windows.SelectMany(window => window.Children).ToArray();
        var unsupported = children.Select(child => child.Tag)
            .Distinct(StringComparer.Ordinal)
            .Where(tag => tag is not ("BUTN" or "APFM" or "EBOX"))
            .ToArray();
        if (unsupported.Length != 0)
            throw new InvalidDataException(
                $"{family} windows contain unsupported child tags: {string.Join(", ", unsupported)}.");
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
            throw new InvalidDataException(
                $"A {family} child record contains the wrong embedded resource number.");
        var catalog = new PackedUiCatalog(windows, buttons, frames, editBoxes);
        var target = Path.Combine(stagingRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target)) catalog.Write(output);
        await using var verify = File.OpenRead(target);
        PackedUiCatalog.Read(verify, relativePath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, SourcePath,
            "application/vnd.dark-sun-wake-redux.ui-catalog",
            $"WIND#{string.Join(',', windowResourceNumbers)} " +
            $"resolved child graph -> DSUI v{PackedUiCatalog.FormatVersion}");
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
        CancellationToken cancellationToken,
        string sourcePath = SourcePath)
    {
        var target = Path.Combine(stagingRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using (var output = File.Create(target))
            PackedIndexedImage.From(image, palette).Write(output);
        await using var verify = File.OpenRead(target);
        PackedIndexedImage.Read(verify, relativePath);
        verify.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(verify, cancellationToken));
        return new(relativePath, verify.Length, hash, sourcePath,
            "application/vnd.dark-sun-wake-redux.indexed-image",
            $"{sourceMapping} -> DSIX v{PackedIndexedImage.FormatVersion}");
    }
}
