using System.Security.Cryptography;
using System.Text;
using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Extractor;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartupAssetExtractorTests
{
    [Fact]
    public void StartMenuAssetMappingsAreUniqueAndBounded()
    {
        Assert.Equal(4, OriginalContent.StartMenuButtons.Count);
        Assert.Equal(2, OriginalContent.StartMenuLayers.Count);
        Assert.Equal(
        [
            new UiLayerAsset("stone-shell", "images/start-menu/stone-shell.dsix",
                20029, 3, 44, 314, 112),
            new UiLayerAsset("flame-ornament", "images/start-menu/flame-ornament.dsix",
                20028, 47, 24, 222, 33)
        ], OriginalContent.StartMenuLayers);
        Assert.Equal(
        [
            new UiLayerAsset("party-overview-base", "images/party-overview/base.dsix",
                11000, 0, 0, 320, 200),
            new UiLayerAsset("view-character-title",
                "images/party-overview/view-character-title.dsix", 20079, 55, 0, 210, 23)
        ], OriginalContent.PartyOverviewLayers);
        Assert.Equal(7, OriginalContent.AddExistingCharacterAssets.Count);
        Assert.Equal(17, OriginalContent.AddExistingCharacterPlacements.Count);
        Assert.Equal(7, OriginalContent.AddExistingCharacterAssets
            .Select(asset => asset.Path).Distinct().Count());
        Assert.All(OriginalContent.AddExistingCharacterAssets, asset =>
        {
            Assert.Contains(asset.Tag, new[] { "BMP ", "ICON" });
            Assert.InRange(asset.FrameWidth, 1, 320);
            Assert.InRange(asset.FrameHeight, 1, 200);
            Assert.InRange(asset.FrameCount, 1, 4);
        });
        var addAssetPaths = OriginalContent.AddExistingCharacterAssets
            .Select(asset => asset.Path)
            .Append("images/character-generation/exit.dsix")
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(OriginalContent.AddExistingCharacterPlacements, placement =>
            Assert.Contains(placement.AssetPath, addAssetPaths));
        Assert.All(OriginalContent.StartMenuLayers, layer =>
        {
            Assert.InRange(layer.X, 0, 319);
            Assert.InRange(layer.Y, 0, 199);
            Assert.InRange(layer.X + layer.FrameWidth, 1, 320);
            Assert.InRange(layer.Y + layer.FrameHeight, 1, 200);
        });
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.Path).Distinct().Count());
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.ButtonResourceNumber).Distinct().Count());
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.ImageResourceNumber).Distinct().Count());
        Assert.All(OriginalContent.StartMenuButtons, button =>
        {
            Assert.InRange(button.FrameWidth, 1, 320);
            Assert.InRange(button.FrameHeight, 1, 200);
        });
        var load = Assert.Single(OriginalContent.StartMenuButtons,
            button => button.Name == "load-saved-game");
        Assert.Equal((191, 13), (load.FrameWidth, load.FrameHeight));
        Assert.Equal(10, OriginalContent.CharacterGenerationButtons.Count);
        Assert.Equal(10, OriginalContent.CharacterGenerationButtons.Select(button => button.Path).Distinct().Count());
        Assert.All(OriginalContent.CharacterGenerationButtons, button =>
        {
            Assert.InRange(button.X + button.ControlWidth, 1, 320);
            Assert.InRange(button.Y + button.ControlHeight, 1, 200);
            Assert.InRange(button.X + button.FrameWidth, 1, 320);
            Assert.InRange(button.Y + button.FrameHeight, 1, 200);
        });
        Assert.Equal(10, OriginalContent.CharacterGenerationModalButtons.Count);
        Assert.Equal(10, OriginalContent.CharacterGenerationModalButtons
            .Select(button => button.Path).Distinct().Count());
        Assert.All(OriginalContent.CharacterGenerationModalButtons,
            button => Assert.Equal(3, button.FrameCount));
        Assert.Equal(new UiLayerAsset("game-menu", "images/game-menu/base.dsix",
            10000, 55, 42, 210, 116), OriginalContent.GameMenuLayer);
        Assert.Equal(new UiLayerAsset("inventory", "images/exploration/inventory-base.dsix",
            13001, 0, 0, 320, 200), OriginalContent.InventoryLayer);
        Assert.Equal([11500U, 13500U],
            OriginalContent.ExplorationDestinationWindowResourceNumbers);
        Assert.Equal(14, OriginalContent.GameMenuButtons.Count);
        Assert.Equal(14, OriginalContent.GameMenuButtons
            .Select(button => button.Path).Distinct().Count());
        Assert.Equal(14, OriginalContent.GameMenuButtons
            .Select(button => button.ButtonResourceNumber).Distinct().Count());
        Assert.Equal(14, OriginalContent.GameMenuButtons
            .Select(button => button.ImageResourceNumber).Distinct().Count());
        Assert.All(OriginalContent.GameMenuButtons, button =>
        {
            Assert.InRange(button.X + button.FrameWidth, 1,
                OriginalContent.GameMenuLayer.FrameWidth);
            Assert.InRange(button.Y + button.FrameHeight, 1,
                OriginalContent.GameMenuLayer.FrameHeight);
            Assert.InRange(button.FrameCount, 2, 4);
        });
    }

    [Fact]
    public async Task InstallsMappedStartupImagesAsVerifiedDerivedAssets()
    {
        var root = Path.Combine(Path.GetTempPath(), "dark-sun-title-tests", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var output = Path.Combine(root, "pack");
        Directory.CreateDirectory(sourceRoot);
        try
        {
            var sourcePath = Path.Combine(sourceRoot, StartupAssetExtractor.SourcePath);
            await File.WriteAllBytesAsync(sourcePath, StartupArchive(), TestContext.Current.CancellationToken);
            var characterSourcePath = Path.Combine(sourceRoot, StartupAssetExtractor.CharacterSourcePath);
            await File.WriteAllBytesAsync(
                characterSourcePath, CharacterArchive(), TestContext.Current.CancellationToken);
            var objectSourcePath = Path.Combine(sourceRoot, StartupAssetExtractor.ObjectSourcePath);
            await File.WriteAllBytesAsync(objectSourcePath, GffRegionTests.ObjectArchive(),
                TestContext.Current.CancellationToken);
            var regionSourcePath = Path.Combine(sourceRoot, StartupAssetExtractor.TyrRegionSourcePath);
            await File.WriteAllBytesAsync(regionSourcePath, GffRegionTests.RegionArchive(),
                TestContext.Current.CancellationToken);
            var edition = new SourceManifest(OriginalContent.GameId, "synthetic-title-edition",
                [await FingerprintAsync(sourceRoot, StartupAssetExtractor.SourcePath),
                    await FingerprintAsync(sourceRoot, StartupAssetExtractor.ObjectSourcePath),
                    await FingerprintAsync(sourceRoot, StartupAssetExtractor.TyrRegionSourcePath)]);

            var manifest = await AssetPackInstaller.InstallAsync(output, staging =>
                StartupAssetExtractor.WritePackAsync(sourceRoot, staging, edition, "test"));

            Assert.Equal(62, manifest.Files.Count);
            var asset = Assert.Single(manifest.Files, item => item.Path == OriginalContent.TitleImageAssetPath);
            Assert.Contains("BMP #11011", asset.Conversion, StringComparison.Ordinal);
            Assert.Contains("PAL #11011", asset.Conversion, StringComparison.Ordinal);
            foreach (var layer in OriginalContent.StartMenuLayers)
            {
                var layerAsset = Assert.Single(manifest.Files, item => item.Path == layer.Path);
                Assert.Contains($"BMP #{layer.ImageResourceNumber}", layerAsset.Conversion,
                    StringComparison.Ordinal);
                Assert.Contains("PAL #1000", layerAsset.Conversion, StringComparison.Ordinal);
            }
            foreach (var layer in OriginalContent.PartyOverviewLayers)
            {
                var layerAsset = Assert.Single(manifest.Files, item => item.Path == layer.Path);
                Assert.Contains($"BMP #{layer.ImageResourceNumber}", layerAsset.Conversion,
                    StringComparison.Ordinal);
                Assert.Contains("PAL #1000", layerAsset.Conversion, StringComparison.Ordinal);
            }
            foreach (var image in OriginalContent.AddExistingCharacterAssets)
            {
                var imageAsset = Assert.Single(manifest.Files, item => item.Path == image.Path);
                Assert.Contains($"{image.Tag}#{image.ResourceNumber}",
                    imageAsset.Conversion, StringComparison.Ordinal);
                Assert.Contains("PAL #1000", imageAsset.Conversion, StringComparison.Ordinal);
                using var imageStream = File.OpenRead(Path.Combine(output,
                    image.Path.Replace('/', Path.DirectorySeparatorChar)));
                Assert.Equal(image.FrameCount, PackedIndexedImage.Read(imageStream).Frames.Count);
            }
            foreach (var button in OriginalContent.StartMenuButtons)
            {
                var buttonAsset = Assert.Single(manifest.Files, item => item.Path == button.Path);
                Assert.Contains($"ICON#{button.ImageResourceNumber}", buttonAsset.Conversion, StringComparison.Ordinal);
                Assert.Contains("PAL #1000", buttonAsset.Conversion, StringComparison.Ordinal);
                using var buttonStream = File.OpenRead(Path.Combine(output,
                    button.Path.Replace('/', Path.DirectorySeparatorChar)));
                Assert.Equal(4, PackedIndexedImage.Read(buttonStream).Frames.Count);
            }
            foreach (var button in OriginalContent.CharacterGenerationButtons)
            {
                var buttonAsset = Assert.Single(manifest.Files, item => item.Path == button.Path);
                Assert.Contains($"ICON#{button.ImageResourceNumber}", buttonAsset.Conversion, StringComparison.Ordinal);
                using var buttonStream = File.OpenRead(Path.Combine(output,
                    button.Path.Replace('/', Path.DirectorySeparatorChar)));
                Assert.Equal(button.FrameCount, PackedIndexedImage.Read(buttonStream).Frames.Count);
            }
            foreach (var button in OriginalContent.CharacterGenerationModalButtons)
            {
                var buttonAsset = Assert.Single(manifest.Files, item => item.Path == button.Path);
                Assert.Contains($"ICON#{button.ImageResourceNumber}", buttonAsset.Conversion, StringComparison.Ordinal);
            }
            var gameMenuLayer = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.GameMenuLayer.Path);
            Assert.Contains("BMP #10000", gameMenuLayer.Conversion, StringComparison.Ordinal);
            Assert.Contains("PAL #1000", gameMenuLayer.Conversion, StringComparison.Ordinal);
            foreach (var button in OriginalContent.GameMenuButtons)
            {
                var buttonAsset = Assert.Single(manifest.Files, item => item.Path == button.Path);
                Assert.Contains($"ICON#{button.ImageResourceNumber}",
                    buttonAsset.Conversion, StringComparison.Ordinal);
                using var buttonStream = File.OpenRead(Path.Combine(output,
                    button.Path.Replace('/', Path.DirectorySeparatorChar)));
                Assert.Equal(button.FrameCount,
                    PackedIndexedImage.Read(buttonStream).Frames.Count);
            }
            var inventoryLayer = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.InventoryLayer.Path);
            Assert.Contains("BMP #13001", inventoryLayer.Conversion, StringComparison.Ordinal);
            Assert.Contains("PAL #1000", inventoryLayer.Conversion, StringComparison.Ordinal);
            var windowAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.PartyWindowImageAssetPath);
            Assert.Contains("BMP #19004", windowAsset.Conversion, StringComparison.Ordinal);
            var fontAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.InterfaceFontAssetPath);
            Assert.Contains("FONT#100", fontAsset.Conversion, StringComparison.Ordinal);
            using (var fontStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.InterfaceFontAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                Assert.Equal(IndexedBitmapFont.CharacterCount,
                    PackedIndexedBitmapFont.Read(fontStream).Glyphs.Count);
            }
            using (var textStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.TextCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                Assert.Equal(["Synthetic"], PackedTextCatalog.Read(textStream).Resources[7]);
            }
            var characterAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.CharacterCatalogAssetPath);
            Assert.Equal(StartupAssetExtractor.CharacterSourcePath, characterAsset.SourcePath);
            Assert.Contains("DSCH v1", characterAsset.Conversion, StringComparison.Ordinal);
            using (var characterStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.CharacterCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                var character = Assert.Single(PackedCharacterCatalog.Read(characterStream).Characters);
                Assert.Equal("Hero", character.Name);
                Assert.Equal(15, character.AbilityScores.Strength);
                Assert.Equal(1, character.RawPsionicMask);
            }
            var regionAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.TyrRegionAssetPath);
            Assert.Equal(StartupAssetExtractor.TyrRegionSourcePath, regionAsset.SourcePath);
            Assert.Contains("OBJEX.GFF:OJFF", regionAsset.Conversion, StringComparison.Ordinal);
            Assert.Contains("DSRG v1", regionAsset.Conversion, StringComparison.Ordinal);
            using (var regionStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.TyrRegionAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                var region = PackedRegion.Read(regionStream);
                Assert.Equal((50U, "Tyr", 2, 2),
                    (region.ResourceNumber, region.Name, region.Tiles.Count, region.Entities.Count));
            }
            var objectAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.TyrObjectCatalogAssetPath);
            Assert.Equal(StartupAssetExtractor.ObjectSourcePath, objectAsset.SourcePath);
            Assert.Contains("RGN032.GFF:ETAB", objectAsset.Conversion, StringComparison.Ordinal);
            Assert.Contains("OBJEX.GFF:OJFF/BMP", objectAsset.Conversion, StringComparison.Ordinal);
            Assert.Contains("DSOB v1", objectAsset.Conversion, StringComparison.Ordinal);
            using (var objectStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.TyrObjectCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                var objects = PackedObjectFrameCatalog.Read(objectStream);
                Assert.Equal((2, 1, 1), (objects.Definitions.Count, objects.Images.Count,
                    objects.Images.Sum(image => image.Frames.Count)));
                Assert.Equal([10U, 11U],
                    objects.Definitions.Select(item => item.ResourceNumber));
            }
            var leaderAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.OpeningLeaderImageAssetPath);
            Assert.Equal(StartupAssetExtractor.ObjectSourcePath, leaderAsset.SourcePath);
            Assert.Contains("OJFF #305", leaderAsset.Conversion, StringComparison.Ordinal);
            Assert.Contains("BMP #599", leaderAsset.Conversion, StringComparison.Ordinal);
            Assert.Contains("RGN032.GFF:PAL #50", leaderAsset.Conversion, StringComparison.Ordinal);
            using (var leaderStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.OpeningLeaderImageAssetPath.Replace(
                           '/', Path.DirectorySeparatorChar))))
            {
                var leader = PackedIndexedImage.Read(leaderStream);
                Assert.Equal(OpeningTyrScene.LeaderFrameGeometry,
                    leader.Frames.Select(frame => (frame.Width, frame.Height)));
                Assert.Equal((byte)0, leader.Palette[0].Red);
            }
            var uiAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.StartFlowUiCatalogAssetPath);
            Assert.Contains("WIND#18501,19500,19501,19502,19503,19504,19505",
                uiAsset.Conversion, StringComparison.Ordinal);
            using (var uiStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.StartFlowUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                var ui = PackedUiCatalog.Read(uiStream);
                Assert.Equal(OriginalContent.StartFlowWindowResourceNumbers,
                    ui.Windows.Select(window => window.ResourceNumber));
                var addWindow = UiWindowGraphResolver.Resolve(ui, 18501);
                Assert.Equal((320, 181, 17),
                    ((int)addWindow.Width, (int)addWindow.Height, addWindow.Controls.Count));
            }
            var gameMenuUiAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.GameMenuUiCatalogAssetPath);
            Assert.Contains("WIND#10500", gameMenuUiAsset.Conversion, StringComparison.Ordinal);
            using (var uiStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.GameMenuUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                var ui = PackedUiCatalog.Read(uiStream);
                Assert.Equal(OriginalContent.GameMenuWindowResourceNumber,
                    Assert.Single(ui.Windows).ResourceNumber);
                Assert.Equal(14, UiWindowGraphResolver.Resolve(ui,
                    OriginalContent.GameMenuWindowResourceNumber).Controls.Count(control =>
                        control.Kind == ResolvedUiControlKind.Button));
            }
            var destinationUiAsset = Assert.Single(manifest.Files,
                item => item.Path == OriginalContent.ExplorationDestinationUiCatalogAssetPath);
            Assert.Contains("WIND#11500,13500", destinationUiAsset.Conversion,
                StringComparison.Ordinal);
            using (var uiStream = File.OpenRead(Path.Combine(output,
                       OriginalContent.ExplorationDestinationUiCatalogAssetPath.Replace(
                           '/', Path.DirectorySeparatorChar))))
            {
                var ui = PackedUiCatalog.Read(uiStream);
                Assert.Equal(OriginalContent.ExplorationDestinationWindowResourceNumbers,
                    ui.Windows.Select(window => window.ResourceNumber));
                Assert.All(new[] { ExplorationView.ViewCharacter, ExplorationView.ViewInventory },
                    view => Assert.Equal(5,
                        ExplorationDestinationInput.Resolve(ui, view).Count));
            }
            Assert.Empty(await OriginalContent.VerifyInstalledAsync(
                output, TestContext.Current.CancellationToken));
            using var packedStream = File.OpenRead(Path.Combine(output, "images", "title.dsix"));
            var packed = PackedIndexedImage.Read(packedStream);
            var frame = Assert.Single(packed.Frames);
            Assert.Equal(320, frame.Width);
            Assert.Equal(200, frame.Height);
            Assert.Equal((byte)4, packed.Palette[0].Red);
            using var shellStream = File.OpenRead(Path.Combine(output,
                OriginalContent.StartMenuLayers[0].Path.Replace('/', Path.DirectorySeparatorChar)));
            Assert.Equal((byte)8, PackedIndexedImage.Read(shellStream).Palette[0].Red);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RejectsMissingCharacterStorageBeforeWritingAssets()
    {
        var root = Path.Combine(Path.GetTempPath(), "dark-sun-title-tests", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var stagingRoot = Path.Combine(root, "staging");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(stagingRoot);
        try
        {
            var sourcePath = Path.Combine(sourceRoot, StartupAssetExtractor.SourcePath);
            await File.WriteAllBytesAsync(
                sourcePath, StartupArchive(), TestContext.Current.CancellationToken);
            string hash;
            await using (var sourceStream = File.OpenRead(sourcePath))
                hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(
                    sourceStream, TestContext.Current.CancellationToken));
            var edition = new SourceManifest(OriginalContent.GameId, "synthetic-title-edition",
                [new SourceFile(StartupAssetExtractor.SourcePath, new FileInfo(sourcePath).Length, hash)]);

            var exception = await Assert.ThrowsAsync<FileNotFoundException>(() =>
                StartupAssetExtractor.WritePackAsync(
                    sourceRoot, stagingRoot, edition, "test", TestContext.Current.CancellationToken));

            Assert.Contains(StartupAssetExtractor.CharacterSourcePath, exception.Message);
            Assert.Empty(Directory.EnumerateFileSystemEntries(stagingRoot));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static byte[] StartupArchive()
    {
        var title = TransparentImage(320, 200);
        var windowImage = TransparentImage(96, 9);
        var menuLayers = OriginalContent.StartMenuLayers.Concat(OriginalContent.PartyOverviewLayers)
            .Append(OriginalContent.GameMenuLayer)
            .Append(OriginalContent.InventoryLayer)
            .Select(layer => (layer.ImageResourceNumber,
                Bytes: TransparentImage(layer.FrameWidth, layer.FrameHeight))).ToArray();
        var icons = OriginalContent.StartMenuButtons
            .Select(button => (button.ImageResourceNumber,
                Bytes: TransparentImage(Enumerable.Range(0, 4).Select(index =>
                    index == 2 && button.ThirdFrameIsPlaceholder
                        ? (Width: 1, Height: 1)
                        : (Width: button.FrameWidth, Height: button.FrameHeight)).ToArray())))
            .ToArray();
        var addExistingImages = OriginalContent.AddExistingCharacterAssets
            .Select(asset => (Asset: asset,
                Bytes: TransparentImage(Enumerable.Repeat(
                    (Width: asset.FrameWidth, Height: asset.FrameHeight), asset.FrameCount).ToArray())))
            .ToArray();
        var characterIcons = OriginalContent.CharacterGenerationButtons
            .Select(button => (button.ImageResourceNumber,
                Bytes: TransparentImage(Enumerable.Repeat(
                    (Width: button.FrameWidth, Height: button.FrameHeight), button.FrameCount).ToArray())))
            .ToArray();
        var modalIcons = OriginalContent.CharacterGenerationModalButtons
            .Select(button => (button.ImageResourceNumber,
                Bytes: TransparentImage(Enumerable.Repeat(
                    (Width: button.FrameWidth, Height: button.FrameHeight), button.FrameCount).ToArray())))
            .ToArray();
        var gameMenuIcons = OriginalContent.GameMenuButtons
            .Select(button => (button.ImageResourceNumber,
                Bytes: TransparentImage(Enumerable.Repeat(
                    (Width: button.FrameWidth, Height: button.FrameHeight),
                    button.FrameCount).ToArray())))
            .ToArray();
        var titlePalette = new byte[IndexedPalette.EncodedLength];
        titlePalette[0] = 1;
        var interfacePalette = new byte[IndexedPalette.EncodedLength];
        interfacePalette[0] = 2;
        var font = Font();
        var text = Encoding.ASCII.GetBytes("Synthetic\r\n");
        var windows = OriginalContent.StartFlowWindowResourceNumbers
            .Append(OriginalContent.GameMenuWindowResourceNumber)
            .Concat(OriginalContent.ExplorationDestinationWindowResourceNumbers)
            .Select(number => (Number: number, Bytes: Window(number))).ToArray();
        var windowButtons = AddExistingButtons().Concat(GameMenuButtons())
            .Select(button => (Number: button.ResourceNumber, Bytes: Button(button))).ToArray();
        var applicationFrames = GameMenuFrames()
            .Select(frame => (Number: frame.ResourceNumber, Bytes: ApplicationFrame(frame))).ToArray();
        var addWindowEditBox = new UiEditBoxResource(18401, 164, 12, 10);
        var addWindowEditBoxes = new[]
        {
            (Number: addWindowEditBox.ResourceNumber, Bytes: EditBox(addWindowEditBox))
        };
        var indexOffset = 28 + title.Length + windowImage.Length + menuLayers.Sum(item => item.Bytes.Length) +
            addExistingImages.Sum(item => item.Bytes.Length) +
            icons.Sum(item => item.Bytes.Length) + characterIcons.Sum(item => item.Bytes.Length) +
            titlePalette.Length + interfacePalette.Length +
            modalIcons.Sum(item => item.Bytes.Length) + gameMenuIcons.Sum(item => item.Bytes.Length) +
            font.Length + text.Length + windows.Sum(item => item.Bytes.Length) +
            windowButtons.Sum(item => item.Bytes.Length) +
            applicationFrames.Sum(item => item.Bytes.Length) +
            addWindowEditBoxes.Sum(item => item.Bytes.Length);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("GFFI"));
        writer.Write(0x0003_0000U);
        writer.Write(28U);
        writer.Write((uint)indexOffset);
        writer.Write(new byte[12]);
        var titleOffset = checked((int)stream.Position);
        writer.Write(title);
        var windowImageOffset = checked((int)stream.Position);
        writer.Write(windowImage);
        var bitmapEntries = new List<(uint Number, int Offset, int Size)>
        {
            (StartupAssetExtractor.TitleImageNumber, titleOffset, title.Length),
            (StartupAssetExtractor.PartyWindowImageNumber, windowImageOffset, windowImage.Length)
        };
        foreach (var layer in menuLayers)
        {
            var offset = checked((int)stream.Position);
            writer.Write(layer.Bytes);
            bitmapEntries.Add((layer.ImageResourceNumber, offset, layer.Bytes.Length));
        }
        var iconEntries = new List<(uint Number, int Offset, int Size)>();
        foreach (var image in addExistingImages)
        {
            var offset = checked((int)stream.Position);
            writer.Write(image.Bytes);
            var entry = (image.Asset.ResourceNumber, offset, image.Bytes.Length);
            if (image.Asset.Tag == "BMP ") bitmapEntries.Add(entry);
            else iconEntries.Add(entry);
        }
        foreach (var icon in icons)
        {
            var offset = checked((int)stream.Position);
            writer.Write(icon.Bytes);
            iconEntries.Add((icon.ImageResourceNumber, offset, icon.Bytes.Length));
        }
        foreach (var icon in characterIcons)
        {
            var offset = checked((int)stream.Position);
            writer.Write(icon.Bytes);
            iconEntries.Add((icon.ImageResourceNumber, offset, icon.Bytes.Length));
        }
        foreach (var icon in modalIcons)
        {
            var offset = checked((int)stream.Position);
            writer.Write(icon.Bytes);
            iconEntries.Add((icon.ImageResourceNumber, offset, icon.Bytes.Length));
        }
        foreach (var icon in gameMenuIcons)
        {
            var offset = checked((int)stream.Position);
            writer.Write(icon.Bytes);
            iconEntries.Add((icon.ImageResourceNumber, offset, icon.Bytes.Length));
        }
        var titlePaletteOffset = checked((int)stream.Position);
        writer.Write(titlePalette);
        var interfacePaletteOffset = checked((int)stream.Position);
        writer.Write(interfacePalette);
        var fontOffset = checked((int)stream.Position);
        writer.Write(font);
        var textOffset = checked((int)stream.Position);
        writer.Write(text);
        var windowEntries = new List<(uint Number, int Offset, int Size)>();
        foreach (var window in windows)
        {
            var offset = checked((int)stream.Position);
            writer.Write(window.Bytes);
            windowEntries.Add((window.Number, offset, window.Bytes.Length));
        }
        var buttonEntries = new List<(uint Number, int Offset, int Size)>();
        foreach (var button in windowButtons)
        {
            var offset = checked((int)stream.Position);
            writer.Write(button.Bytes);
            buttonEntries.Add((button.Number, offset, button.Bytes.Length));
        }
        var applicationFrameEntries = new List<(uint Number, int Offset, int Size)>();
        foreach (var frame in applicationFrames)
        {
            var offset = checked((int)stream.Position);
            writer.Write(frame.Bytes);
            applicationFrameEntries.Add((frame.Number, offset, frame.Bytes.Length));
        }
        var editBoxEntries = new List<(uint Number, int Offset, int Size)>();
        foreach (var editBox in addWindowEditBoxes)
        {
            var offset = checked((int)stream.Position);
            writer.Write(editBox.Bytes);
            editBoxEntries.Add((editBox.Number, offset, editBox.Bytes.Length));
        }
        writer.Write(0U);
        writer.Write(0U);
        writer.Write((ushort)9);
        WriteTable(writer, "BMP ", bitmapEntries);
        WriteTable(writer, "ICON", iconEntries);
        WriteTable(writer, "PAL ",
        [
            (StartupAssetExtractor.TitlePaletteNumber, titlePaletteOffset, titlePalette.Length),
            (StartupAssetExtractor.InterfacePaletteNumber, interfacePaletteOffset, interfacePalette.Length)
        ]);
        WriteTable(writer, "FONT", [(StartupAssetExtractor.FontNumber, fontOffset, font.Length)]);
        WriteTable(writer, "TEXT", [(7U, textOffset, text.Length)]);
        WriteTable(writer, "WIND", windowEntries);
        WriteTable(writer, "BUTN", buttonEntries);
        WriteTable(writer, "APFM", applicationFrameEntries);
        WriteTable(writer, "EBOX", editBoxEntries);
        return stream.ToArray();
    }

    private static byte[] CharacterArchive()
    {
        var character = new byte[GffCharacterRecordEnvelope.FixedHeaderSize];
        character[0] = GffCharacterRecordEnvelope.SupportedVersion;
        Array.Fill(character, (byte)15, GffCharacterAbilityScores.ScoresOffset,
            GffCharacterAbilityScores.ScoreCount);
        Encoding.ASCII.GetBytes("Hero").CopyTo(character, GffCharacterIdentity.NameOffset);
        var indexOffset = GffArchive.HeaderSize + character.Length + 1;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("GFFI"));
        writer.Write(0x0003_0000U);
        writer.Write((uint)GffArchive.HeaderSize);
        writer.Write((uint)indexOffset);
        writer.Write(new byte[12]);
        var characterOffset = checked((int)stream.Position);
        writer.Write(character);
        var psionicOffset = checked((int)stream.Position);
        writer.Write((byte)1);
        writer.Write(0U);
        writer.Write(0U);
        writer.Write((ushort)2);
        WriteTable(writer, "CHAR", [(7U, characterOffset, character.Length)]);
        WriteTable(writer, "PSIN", [(7U, psionicOffset, 1)]);
        return stream.ToArray();
    }

    private static async Task<SourceFile> FingerprintAsync(string root, string relativePath)
    {
        var path = Path.Combine(root, relativePath);
        await using var stream = File.OpenRead(path);
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(
            stream, TestContext.Current.CancellationToken));
        return new(relativePath, stream.Length, hash);
    }

    private static byte[] Window(uint number)
    {
        var children = number switch
        {
            18501 => AddExistingChildren(),
            OriginalContent.GameMenuWindowResourceNumber => GameMenuChildren(),
            11500 => DestinationChildren(
                [(43, 155), (67, 155), (91, 155), (114, 155), (253, 155)]),
            13500 => DestinationChildren(
                [(163, 181), (187, 181), (211, 181), (235, 181), (288, 181)]),
            _ => []
        };
        var bytes = new byte[UiWindowResource.FixedSize + children.Length * UiWindowResource.ChildRecordSize];
        Encoding.ASCII.GetBytes("WIND").CopyTo(bytes, 0);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);
        BitConverter.GetBytes(number).CopyTo(bytes, 8);
        BitConverter.GetBytes(number switch
            {
                18501 => 10002U,
                OriginalContent.GameMenuWindowResourceNumber or 11500 or 13500 => 0U,
                _ => StartupAssetExtractor.PartyWindowImageNumber
            })
            .CopyTo(bytes, 58);
        BitConverter.GetBytes(checked((ushort)(number == OriginalContent.GameMenuWindowResourceNumber
            ? OriginalContent.GameMenuLayer.FrameWidth : 320))).CopyTo(bytes, 190);
        BitConverter.GetBytes(checked((ushort)(number switch
            {
                18501 => 181,
                OriginalContent.GameMenuWindowResourceNumber =>
                    OriginalContent.GameMenuLayer.FrameHeight,
                11500 => 189,
                _ => 200
            }))).CopyTo(bytes, 192);
        for (var index = 0; index < children.Length; index++)
        {
            var offset = UiWindowResource.FixedSize + index * UiWindowResource.ChildRecordSize;
            Encoding.ASCII.GetBytes(children[index].Tag).CopyTo(bytes, offset + 4);
            BitConverter.GetBytes(children[index].ResourceNumber).CopyTo(bytes, offset + 8);
            BitConverter.GetBytes(children[index].X).CopyTo(bytes, offset + 12);
            BitConverter.GetBytes(children[index].Y).CopyTo(bytes, offset + 14);
        }
        return bytes;
    }

    private static UiChildReference[] AddExistingChildren() =>
    [
        new("BUTN", 18300, 110, 0),
        new("BUTN", 18301, 231, 30),
        new("BUTN", 18302, 231, 50),
        new("BUTN", 18303, 215, 148),
        new("BUTN", 18304, 46, 31),
        new("BUTN", 18305, 46, 42),
        new("BUTN", 18306, 46, 53),
        new("BUTN", 18307, 46, 64),
        new("BUTN", 18308, 46, 75),
        new("BUTN", 18309, 46, 86),
        new("BUTN", 18310, 46, 97),
        new("BUTN", 18311, 46, 108),
        new("BUTN", 18312, 46, 119),
        new("BUTN", 18313, 46, 130),
        new("BUTN", 10314, 215, 30),
        new("BUTN", 10315, 215, 130),
        new("EBOX", 18401, 49, 147)
    ];

    private static UiButtonResource[] AddExistingButtons() =>
    [
        new(18300, 67, 23, 18107, 4),
        new(18301, 44, 15, 18108, 0),
        new(18302, 44, 15, 18109, 0),
        new(18303, 62, 15, 18110, 0),
        new(18304, 163, 11, 18100, 208),
        new(18305, 163, 11, 18100, 208),
        new(18306, 163, 11, 18100, 208),
        new(18307, 163, 11, 18100, 208),
        new(18308, 163, 11, 18100, 208),
        new(18309, 163, 11, 18100, 208),
        new(18310, 163, 11, 18100, 208),
        new(18311, 163, 11, 18100, 208),
        new(18312, 163, 11, 18100, 208),
        new(18313, 163, 11, 18100, 208),
        new(10314, 14, 14, 12102, 0),
        new(10315, 14, 14, 12101, 0)
    ];

    private static UiChildReference[] GameMenuChildren() =>
    [
        new("APFM", 11270, 18, 102),
        new("BUTN", 10308, 139, 78),
        new("BUTN", 10300, 49, 24),
        new("BUTN", 11304, 81, 24),
        new("BUTN", 11305, 113, 24),
        new("BUTN", 11306, 145, 24),
        new("BUTN", 10301, 49, 51),
        new("BUTN", 10302, 73, 51),
        new("BUTN", 10303, 97, 51),
        new("BUTN", 10305, 121, 51),
        new("BUTN", 10306, 145, 51),
        new("BUTN", 10310, 76, 78),
        new("BUTN", 10311, 97, 78),
        new("BUTN", 10312, 116, 78),
        new("BUTN", 10313, 44, 78),
        new("APFM", 10201, 40, 18),
        new("APFM", 10214, 139, 78),
        new("APFM", 10215, 49, 24),
        new("APFM", 10216, 81, 24),
        new("APFM", 10217, 113, 24),
        new("APFM", 10218, 145, 24),
        new("APFM", 10202, 49, 51),
        new("APFM", 10203, 73, 51),
        new("APFM", 10204, 97, 51),
        new("APFM", 10210, 121, 51),
        new("APFM", 10211, 145, 51),
        new("APFM", 10205, 76, 78),
        new("APFM", 10206, 97, 78),
        new("APFM", 10207, 116, 78),
        new("APFM", 10208, 44, 78)
    ];

    private static UiChildReference[] DestinationChildren(
        IReadOnlyList<(short X, short Y)> positions)
    {
        uint[] resources = [10300, 11304, 11305, 11306, 10308];
        return resources.Select((resource, index) =>
            new UiChildReference("BUTN", resource,
                positions[index].X, positions[index].Y)).ToArray();
    }

    private static UiButtonResource[] GameMenuButtons() =>
        OriginalContent.GameMenuButtons.Select(asset => new UiButtonResource(
            asset.ButtonResourceNumber, checked((ushort)asset.FrameWidth),
            checked((ushort)asset.FrameHeight), asset.ImageResourceNumber, 0)).ToArray();

    private static UiApplicationFrameResource[] GameMenuFrames() =>
    [
        new(11270, 4, 4, 0),
        new(10201, 132, 78, 110),
        new(10214, 28, 16, 0),
        new(10215, 16, 16, 0),
        new(10216, 16, 16, 0),
        new(10217, 16, 16, 0),
        new(10218, 16, 16, 0),
        new(10202, 16, 16, 0),
        new(10203, 16, 16, 0),
        new(10204, 16, 16, 0),
        new(10210, 16, 16, 0),
        new(10211, 16, 16, 0),
        new(10205, 16, 16, 0),
        new(10206, 16, 16, 0),
        new(10207, 16, 16, 0),
        new(10208, 28, 16, 0)
    ];

    private static byte[] Button(UiButtonResource button)
    {
        var bytes = new byte[UiButtonResource.FixedSize];
        Encoding.ASCII.GetBytes("BUTN").CopyTo(bytes, 0);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);
        BitConverter.GetBytes(button.ResourceNumber).CopyTo(bytes, 8);
        BitConverter.GetBytes(button.Width).CopyTo(bytes, 40);
        BitConverter.GetBytes(button.Height).CopyTo(bytes, 42);
        BitConverter.GetBytes(button.EventMask).CopyTo(bytes, 88);
        BitConverter.GetBytes(button.ResourceNumber).CopyTo(bytes, 90);
        BitConverter.GetBytes(button.ImageResourceNumber).CopyTo(bytes, 100);
        return bytes;
    }

    private static byte[] ApplicationFrame(UiApplicationFrameResource frame)
    {
        var bytes = new byte[UiApplicationFrameResource.RecordSize];
        Encoding.ASCII.GetBytes("APFM").CopyTo(bytes, 0);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);
        BitConverter.GetBytes(frame.ResourceNumber).CopyTo(bytes, 8);
        BitConverter.GetBytes(frame.Width).CopyTo(bytes, 40);
        BitConverter.GetBytes(frame.Height).CopyTo(bytes, 42);
        BitConverter.GetBytes(frame.EventMask).CopyTo(bytes, 88);
        return bytes;
    }

    private static byte[] EditBox(UiEditBoxResource editBox)
    {
        var bytes = new byte[UiEditBoxResource.RecordSize];
        Encoding.ASCII.GetBytes("EBOX").CopyTo(bytes, 0);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 4);
        BitConverter.GetBytes(editBox.ResourceNumber).CopyTo(bytes, 8);
        BitConverter.GetBytes(editBox.ResourceNumber).CopyTo(bytes, 24);
        BitConverter.GetBytes(editBox.Width).CopyTo(bytes, 34);
        BitConverter.GetBytes(editBox.Height).CopyTo(bytes, 36);
        BitConverter.GetBytes(editBox.EventMask).CopyTo(bytes, 150);
        return bytes;
    }

    private static byte[] Font()
    {
        const ushort height = 1;
        var bytes = new byte[IndexedBitmapFont.HeaderSize + IndexedBitmapFont.CharacterCount * 2];
        BitConverter.GetBytes((ushort)IndexedBitmapFont.CharacterCount).CopyTo(bytes, 0);
        BitConverter.GetBytes(height).CopyTo(bytes, 2);
        for (var index = 0; index < IndexedBitmapFont.CharacterCount; index++)
        {
            bytes[8 + index] = (byte)index;
            var offset = IndexedBitmapFont.HeaderSize + index * 2;
            BitConverter.GetBytes(checked((ushort)offset)).CopyTo(bytes, 264 + index * 2);
        }
        return bytes;
    }

    private static byte[] TransparentImage(int width, int height) =>
        TransparentImage([(width, height)]);

    private static byte[] TransparentImage(IReadOnlyList<(int Width, int Height)> frames)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        var tableEnd = 6 + frames.Count * 4;
        var totalSize = tableEnd + frames.Count * 10;
        writer.Write((uint)totalSize);
        writer.Write((ushort)frames.Count);
        for (var index = 0; index < frames.Count; index++) writer.Write((uint)(tableEnd + index * 10));
        foreach (var frame in frames)
        {
            writer.Write((ushort)frame.Width);
            writer.Write((ushort)frame.Height);
            writer.Write((byte)0xff);
            writer.Write(Encoding.ASCII.GetBytes("PLAN"));
            writer.Write((byte)0);
        }
        return stream.ToArray();
    }

    private static void WriteTable(
        BinaryWriter writer,
        string tag,
        IReadOnlyList<(uint Number, int Offset, int Size)> entries)
    {
        writer.Write(Encoding.ASCII.GetBytes(tag));
        writer.Write((uint)entries.Count);
        foreach (var entry in entries)
        {
            writer.Write(entry.Number);
            writer.Write((uint)entry.Offset);
            writer.Write((uint)entry.Size);
        }
    }
}
