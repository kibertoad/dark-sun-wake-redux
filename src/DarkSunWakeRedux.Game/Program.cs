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
            var uiPath = Path.Combine(assetPack,
                OriginalContent.StartFlowUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var uiStream = File.OpenRead(uiPath);
            var ui = PackedUiCatalog.Read(uiStream, OriginalContent.StartFlowUiCatalogAssetPath);
            if (!ui.Windows.Select(window => window.ResourceNumber)
                .SequenceEqual(OriginalContent.StartFlowWindowResourceNumbers))
                throw new InvalidDataException("The installed start-flow UI catalog has unexpected windows.");
            var resolvedControlCount = OriginalContent.StartFlowWindowResourceNumbers.Sum(number =>
                UiWindowGraphResolver.Resolve(ui, number).Controls.Count);
            if (resolvedControlCount != 39)
                throw new InvalidDataException("The installed start-flow UI graph does not resolve all 39 controls.");
            if (StartMenuInput.Resolve(ui).Count != OriginalContent.StartMenuButtons.Count)
                throw new InvalidDataException("The installed start-window UI graph is incomplete.");
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
