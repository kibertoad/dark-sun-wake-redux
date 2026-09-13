using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;

try
{
    if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase)) return 0;
    var platformSmoke = args.Contains("--platform-smoke-test", StringComparer.OrdinalIgnoreCase);
    var contentSmoke = args.Contains("--content-smoke-test", StringComparer.OrdinalIgnoreCase);
    string? assetPack = null;
    if (!platformSmoke)
    {
        assetPack = Option(args, "--asset-pack") ?? OriginalContent.DefaultAssetPackPath();
        var diagnostics = await OriginalContent.VerifyInstalledAsync(assetPack);
        if (diagnostics.Count != 0)
        {
            var details = string.Join(Environment.NewLine,
                diagnostics.Select(diagnostic => $"[{diagnostic.Code}] {diagnostic.Message}"));
            throw new InvalidDataException(
                $"A verified local asset pack is required at '{assetPack}'." + Environment.NewLine +
                "Run DarkSunWakeRedux.Extractor against your legally owned GOG installation." +
                Environment.NewLine + details);
        }
        if (contentSmoke)
        {
            var titlePath = Path.Combine(assetPack,
                OriginalContent.TitleImageAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var titleStream = File.OpenRead(titlePath);
            var title = PackedIndexedImage.Read(titleStream, OriginalContent.TitleImageAssetPath);
            var titleFrame = title.Frames.Single();
            if (titleFrame.Width != 320 || titleFrame.Height != 200)
                throw new InvalidDataException("The installed title image is not the required 320x200 frame.");
            foreach (var layer in OriginalContent.StartMenuLayers.Concat(OriginalContent.PartyOverviewLayers))
            {
                var image = ReadImage(assetPack, layer.Path);
                var frame = AssertSingleFrame(image.Frames, layer.Name);
                if (frame.Width != layer.FrameWidth || frame.Height != layer.FrameHeight)
                    throw new InvalidDataException(
                        $"The installed {layer.Name} image has unexpected geometry.");
            }
            foreach (var asset in OriginalContent.AddExistingCharacterAssets)
            {
                var image = ReadImage(assetPack, asset.Path);
                if (!asset.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed add-existing {asset.Name} image has unexpected geometry.");
            }
            foreach (var button in OriginalContent.StartMenuButtons)
            {
                var buttonPath = Path.Combine(assetPack,
                    button.Path.Replace('/', Path.DirectorySeparatorChar));
                using var buttonStream = File.OpenRead(buttonPath);
                var image = PackedIndexedImage.Read(buttonStream, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException($"The installed {button.Name} image has unexpected geometry.");
            }
            foreach (var button in OriginalContent.CharacterGenerationButtons)
            {
                var buttonPath = Path.Combine(assetPack,
                    button.Path.Replace('/', Path.DirectorySeparatorChar));
                using var buttonStream = File.OpenRead(buttonPath);
                var image = PackedIndexedImage.Read(buttonStream, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed character-generation {button.Name} image has unexpected geometry.");
            }
            foreach (var button in OriginalContent.CharacterGenerationModalButtons)
            {
                var buttonPath = Path.Combine(assetPack,
                    button.Path.Replace('/', Path.DirectorySeparatorChar));
                using var buttonStream = File.OpenRead(buttonPath);
                var image = PackedIndexedImage.Read(buttonStream, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed character-generation modal {button.Name} image has unexpected geometry.");
            }
            var gameMenuLayer = ReadImage(assetPack, OriginalContent.GameMenuLayer.Path);
            var gameMenuFrame = AssertSingleFrame(gameMenuLayer.Frames, "game-menu base");
            if (gameMenuFrame.Width != OriginalContent.GameMenuLayer.FrameWidth ||
                gameMenuFrame.Height != OriginalContent.GameMenuLayer.FrameHeight)
                throw new InvalidDataException("The installed game-menu base has unexpected geometry.");
            foreach (var button in OriginalContent.GameMenuButtons)
            {
                var image = ReadImage(assetPack, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed game-menu {button.Name} image has unexpected geometry.");
            }
            var openingLeader = ReadImage(assetPack, OriginalContent.OpeningLeaderImageAssetPath);
            var openingLeaderFrame = openingLeader.Frames[0];
            if (openingLeaderFrame.Width != OpeningTyrScene.LeaderWidth ||
                openingLeaderFrame.Height != OpeningTyrScene.LeaderHeight ||
                openingLeaderFrame.Alpha.Count(value => value != 0) != 367)
                throw new InvalidDataException(
                    "The installed opening-leader image has unexpected geometry or alpha coverage.");
            var windowImagePath = Path.Combine(assetPack,
                OriginalContent.PartyWindowImageAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using (var windowImageStream = File.OpenRead(windowImagePath))
            {
                var windowImage = PackedIndexedImage.Read(windowImageStream,
                    OriginalContent.PartyWindowImageAssetPath);
                var frame = AssertSingleFrame(windowImage.Frames, "party-window image");
                if (frame.Width != 96 || frame.Height != 9)
                    throw new InvalidDataException("The installed party-window image is not 96x9.");
            }
            var fontPath = Path.Combine(assetPack,
                OriginalContent.InterfaceFontAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var fontStream = File.OpenRead(fontPath);
            var font = PackedIndexedBitmapFont.Read(fontStream, OriginalContent.InterfaceFontAssetPath);
            if (font.Glyphs.Count != IndexedBitmapFont.CharacterCount)
                throw new InvalidDataException("The installed interface font has an unexpected glyph count.");
            var textPath = Path.Combine(assetPack,
                OriginalContent.TextCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var textStream = File.OpenRead(textPath);
            if (PackedTextCatalog.Read(textStream, OriginalContent.TextCatalogAssetPath).Resources.Count == 0)
                throw new InvalidDataException("The installed text catalog is empty.");
            var characterPath = Path.Combine(assetPack,
                OriginalContent.CharacterCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var characterStream = File.OpenRead(characterPath);
            if (PackedCharacterCatalog.Read(
                    characterStream, OriginalContent.CharacterCatalogAssetPath).Characters.Count == 0)
                throw new InvalidDataException("The installed character catalog is empty.");
            var regionPath = Path.Combine(assetPack,
                OriginalContent.TyrRegionAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var regionStream = File.OpenRead(regionPath);
            var region = PackedRegion.Read(regionStream, OriginalContent.TyrRegionAssetPath);
            if (region.ResourceNumber != 50 || region.Name != "Tyr" ||
                region.Tiles.Count != 94 || region.Entities.Count != 867)
                throw new InvalidDataException("The installed Tyr region catalog is incomplete.");
            var geometryCounts = region.GeometryMap.GroupBy(value => value)
                .ToDictionary(group => group.Key, group => group.Count());
            var terrain = new RegionTerrainGrid(region);
            if (geometryCounts.Count != 4 ||
                geometryCounts.GetValueOrDefault((byte)0x00) != 8_131 ||
                geometryCounts.GetValueOrDefault((byte)0x40) != 2_044 ||
                geometryCounts.GetValueOrDefault((byte)0x80) != 38 ||
                geometryCounts.GetValueOrDefault((byte)0xc0) != 2_331 ||
                terrain.OpenCellCount != 8_169 ||
                terrain.IsTerrainOpen(60, 84) || !terrain.IsTerrainOpen(74, 94))
                throw new InvalidDataException(
                    "The installed Tyr geometry plane does not match the verified navigation contract.");
            var occupancy = new ExplorationOccupancySession(
                terrain.Width, terrain.Height,
                point => terrain.IsTerrainOpen(point.X, point.Y));
            var actorPlacement = occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
                1, new(OpeningTyrScene.LeaderAnchorCellX,
                    OpeningTyrScene.LeaderAnchorCellY), GridFootprint.SingleCell));
            if (!actorPlacement.Applied)
                throw new InvalidDataException(
                    "The opening leader cannot occupy its evidenced Tyr anchor.");
            var actor = new ExplorationActorController(terrain, occupancy, 1);
            var exploration = new ExplorationSnapshot(
                OpeningTyrScene.OriginX, OpeningTyrScene.OriginY,
                ExplorationCursorMode.Walk, PartyDisplayMode.LeaderOnly,
                ExplorationView.World);
            var targetCellX = OpeningTyrScene.LeaderAnchorCellX + 1;
            var targetLogicalX = targetCellX * GffRegion.TilePixelSize +
                GffRegion.TilePixelSize / 2 - OpeningTyrScene.OriginX;
            var targetLogicalY = OpeningTyrScene.LeaderAnchorCellY *
                GffRegion.TilePixelSize + GffRegion.TilePixelSize / 2 -
                OpeningTyrScene.OriginY;
            var plannedMovement = actor.PlanAt(
                exploration, targetLogicalX, targetLogicalY);
            var advancedMovement = actor.Advance(
                ExplorationActorController.DefaultStepInterval);
            if (plannedMovement is null || !plannedMovement.Applied ||
                advancedMovement.Count != 1 ||
                actor.Snapshot().Position != new GridPoint(
                    targetCellX, OpeningTyrScene.LeaderAnchorCellY))
                throw new InvalidDataException(
                    "The installed Tyr content does not support the opening actor movement contract.");
            var objectPath = Path.Combine(assetPack,
                OriginalContent.TyrObjectCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var objectStream = File.OpenRead(objectPath);
            var objects = PackedObjectFrameCatalog.Read(
                objectStream, OriginalContent.TyrObjectCatalogAssetPath);
            if (objects.Definitions.Count != 287 || objects.Images.Count != 246 ||
                objects.Images.Sum(image => image.Frames.Count) != 477)
                throw new InvalidDataException("The installed Tyr object-frame catalog is incomplete.");
            var objectNumbers = objects.Definitions.Select(item => item.ResourceNumber).ToHashSet();
            if (region.Entities.Any(entity => !objectNumbers.Contains(entity.ObjectResourceNumber)))
                throw new InvalidDataException("The Tyr region references a missing object definition.");
            var topLeft = RegionSceneRasterizer.Rasterize(region, objects, 0, 0,
                LogicalCanvasTransform.LogicalWidth, LogicalCanvasTransform.LogicalHeight);
            var bottomRight = RegionSceneRasterizer.Rasterize(region, objects,
                RegionSceneRasterizer.WorldWidth - LogicalCanvasTransform.LogicalWidth,
                RegionSceneRasterizer.WorldHeight - LogicalCanvasTransform.LogicalHeight,
                LogicalCanvasTransform.LogicalWidth, LogicalCanvasTransform.LogicalHeight);
            var opening = OpeningTyrScene.Rasterize(region, objects);
            if (topLeft.Pixels.Length != 64_000 || bottomRight.Pixels.Length != 64_000 ||
                opening.Pixels.Length != 64_000)
                throw new InvalidDataException("The Tyr scene compositor returned incomplete viewports.");
            var uiPath = Path.Combine(assetPack,
                OriginalContent.StartFlowUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var uiStream = File.OpenRead(uiPath);
            var ui = PackedUiCatalog.Read(uiStream, OriginalContent.StartFlowUiCatalogAssetPath);
            if (!ui.Windows.Select(window => window.ResourceNumber)
                .SequenceEqual(OriginalContent.StartFlowWindowResourceNumbers))
                throw new InvalidDataException("The installed start-flow UI catalog has unexpected windows.");
            var resolvedControlCount = OriginalContent.StartFlowWindowResourceNumbers.Sum(number =>
                UiWindowGraphResolver.Resolve(ui, number).Controls.Count);
            if (resolvedControlCount != 56)
                throw new InvalidDataException("The installed start-flow UI graph does not resolve all 56 controls.");
            if (StartMenuInput.Resolve(ui).Count != OriginalContent.StartMenuButtons.Count)
                throw new InvalidDataException("The installed start-window UI graph is incomplete.");
            if (AddExistingCharacterInput.Resolve(ui).Count != 17)
                throw new InvalidDataException("The installed ADD-list UI graph is incomplete.");
            _ = PartyOverviewInput.Resolve(ui);
            var gameMenuUiPath = Path.Combine(assetPack,
                OriginalContent.GameMenuUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var gameMenuUiStream = File.OpenRead(gameMenuUiPath);
            var gameMenuUi = PackedUiCatalog.Read(
                gameMenuUiStream, OriginalContent.GameMenuUiCatalogAssetPath);
            if (GameMenuInput.Resolve(gameMenuUi).Count != OriginalContent.GameMenuButtons.Count)
                throw new InvalidDataException("The installed game-menu UI graph is incomplete.");
            Console.WriteLine($"Verified runtime startup assets at {assetPack}.");
            return 0;
        }
    }
    using var game = new DarkSunWakeReduxGame(assetPack, platformSmoke);
    game.Run();
    return 0;
}
catch (Exception exception)
{
    StartupFailureReporter.Report(exception);
    return 1;
}

static string? Option(string[] values, string name)
{
    var index = Array.FindIndex(values, value => value.Equals(name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < values.Length ? values[index + 1] : null;
}

static IndexedImageFrame AssertSingleFrame(IReadOnlyList<IndexedImageFrame> frames, string name) =>
    frames.Count == 1 ? frames[0] : throw new InvalidDataException($"The installed {name} must have one frame.");

static PackedIndexedImage ReadImage(string assetPack, string assetPath)
{
    var path = Path.Combine(assetPack, assetPath.Replace('/', Path.DirectorySeparatorChar));
    using var stream = File.OpenRead(path);
    return PackedIndexedImage.Read(stream, assetPath);
}
