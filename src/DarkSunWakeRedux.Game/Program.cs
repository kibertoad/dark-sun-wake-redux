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
