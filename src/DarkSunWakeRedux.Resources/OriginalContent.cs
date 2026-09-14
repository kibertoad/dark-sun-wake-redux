using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DarkSunWakeRedux.Resources;

public sealed record SourceManifest(string GameId, string SourceEdition, IReadOnlyList<SourceFile> Files)
{
    public static SourceManifest Load(Stream stream)
    {
        var result = JsonSerializer.Deserialize<SourceManifest>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true })
            ?? throw new InvalidDataException("Source manifest is empty.");
        result.Validate();
        return result;
    }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(GameId);
        ArgumentException.ThrowIfNullOrWhiteSpace(SourceEdition);
        if (Files is null || Files.Count == 0)
            throw new InvalidDataException("A source manifest requires at least one fingerprint.");

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Files)
        {
            if (file is null) throw new InvalidDataException("Source manifest contains a null file record.");
            var path = Normalize(file.Path);
            if (!seen.Add(path)) throw new InvalidDataException($"Duplicate source path '{path}'.");
            if (file.Size < 0 || !OriginalContent.IsSha256(file.Sha256))
                throw new InvalidDataException($"Invalid fingerprint for '{path}'.");
        }
    }

    public string Fingerprint()
    {
        var canonical = string.Join('\n', Files.OrderBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
            .Select(file => $"{Normalize(file.Path)}\0{file.Size}\0{file.Sha256.ToLowerInvariant()}"));
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    internal static string Normalize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var normalized = path.Replace('\\', '/');
        if (Path.IsPathRooted(path) || normalized.Split('/').Any(part => part is "" or "." or ".."))
            throw new InvalidDataException($"Source path must be relative: '{path}'.");
        return normalized;
    }
}

public sealed record SourceFile(string Path, long Size, string Sha256);

public sealed record AssetPackFile(
    string Path,
    long Size,
    string Sha256,
    string SourcePath,
    string MediaType,
    string Conversion);

public sealed record AssetPackManifest(
    int FormatVersion,
    string GameId,
    string SourceEdition,
    string SourceFingerprintSha256,
    string ExtractorVersion,
    IReadOnlyList<AssetPackFile> Files);

public sealed record ContentDiagnostic(
    string Code,
    string Message,
    string? Path = null,
    string? Expected = null,
    string? Actual = null);

public sealed record StartMenuButtonAsset(
    string Name,
    uint ButtonResourceNumber,
    string Path,
    uint ImageResourceNumber,
    int FrameWidth,
    int FrameHeight,
    bool ThirdFrameIsPlaceholder)
{
    public bool HasExpectedFrames(IReadOnlyList<IndexedImageFrame> frames) =>
        frames.Count == 4 && frames.Select((frame, index) =>
        {
            var placeholder = index == 2 && ThirdFrameIsPlaceholder;
            return frame.Width == (placeholder ? 1 : FrameWidth) &&
                frame.Height == (placeholder ? 1 : FrameHeight);
        }).All(matches => matches);
}

public sealed record UiLayerAsset(
    string Name,
    string Path,
    uint ImageResourceNumber,
    int X,
    int Y,
    int FrameWidth,
    int FrameHeight);

public sealed record UiImageAsset(
    string Name,
    string Path,
    string Tag,
    uint ResourceNumber,
    int FrameWidth,
    int FrameHeight,
    int FrameCount)
{
    public bool HasExpectedFrames(IReadOnlyList<IndexedImageFrame> frames) =>
        frames.Count == FrameCount && frames.All(frame =>
            frame.Width == FrameWidth && frame.Height == FrameHeight);
}

public sealed record UiImagePlacement(string AssetPath, int X, int Y);

public sealed record CharacterGenerationButtonAsset(
    string Name,
    string Path,
    uint ButtonResourceNumber,
    uint ImageResourceNumber,
    int X,
    int Y,
    int ControlWidth,
    int ControlHeight,
    int FrameWidth,
    int FrameHeight,
    int FrameCount)
{
    public bool HasExpectedFrames(IReadOnlyList<IndexedImageFrame> frames) =>
        frames.Count == FrameCount && frames.All(frame =>
            frame.Width == FrameWidth && frame.Height == FrameHeight);
}

public sealed record GameMenuButtonAsset(
    string Name,
    string Path,
    uint ButtonResourceNumber,
    uint ImageResourceNumber,
    int X,
    int Y,
    int FrameWidth,
    int FrameHeight,
    int FrameCount)
{
    public bool HasExpectedFrames(IReadOnlyList<IndexedImageFrame> frames) =>
        frames.Count == FrameCount && frames.All(frame =>
            frame.Width == FrameWidth && frame.Height == FrameHeight);
}

public sealed record InteractionButtonAsset(
    string Name,
    string Path,
    uint ImageResourceNumber,
    int FrameWidth,
    int FrameHeight,
    int FrameCount)
{
    public bool HasExpectedFrames(IReadOnlyList<IndexedImageFrame> frames) =>
        frames.Count == FrameCount && frames.All(frame =>
            frame.Width == FrameWidth && frame.Height == FrameHeight);
}

public sealed record SourceIdentification(
    SourceManifest? Edition,
    IReadOnlyList<ContentDiagnostic> Diagnostics)
{
    public bool IsSupported => Edition is not null;
}

public static class OriginalContent
{
    public const int AssetPackFormatVersion = 24;
    public const string GameId = "dark-sun-wake-redux";
    public const string TitleImageAssetPath = "images/title.dsix";
    public const string InterfaceFontAssetPath = "fonts/interface.dsft";
    public const string TextCatalogAssetPath = "text/resources.dstx";
    public const string PartyWindowImageAssetPath = "images/party/window-image.dsix";
    public const string StartFlowUiCatalogAssetPath = "ui/start-flow.dsui";
    public const string GameMenuUiCatalogAssetPath = "ui/game-menu.dsui";
    public const string ExplorationDestinationUiCatalogAssetPath =
        "ui/exploration-destinations.dsui";
    public const string InteractionUiCatalogAssetPath = "ui/interaction.dsui";
    public const string FirstTyrDialogueScriptAssetPath = "dialogue/gpl-135.dsgp";
    public const string DialogueGlobalStringsScriptAssetPath = "dialogue/mas-99.dsgp";
    public const string FirstTyrDialoguePortraitAssetPath =
        "images/dialogue/portrait-18.dsix";
    public const string CharacterCatalogAssetPath = "characters/catalog.dsch";
    public const string TyrRegionAssetPath = "regions/tyr.dsrg";
    public const string TyrObjectCatalogAssetPath = "regions/tyr-objects.dsob";
    public const string OpeningLeaderImageAssetPath = "images/exploration/opening-leader.dsix";
    public const uint OpeningLeaderObjectResourceNumber = 305;
    public const uint OpeningLeaderImageResourceNumber = 599;
    public const long MaximumManifestBytes = 4 * 1024 * 1024;

    public static IReadOnlyList<uint> StartFlowWindowResourceNumbers { get; } =
        [18501, 19500, 19501, 19502, 19503, 19504, 19505];

    public const uint GameMenuWindowResourceNumber = 10500;

    public const uint PreferencesWindowResourceNumber = 16500;

    public static IReadOnlyList<uint> GameMenuWindowResourceNumbers { get; } =
        [GameMenuWindowResourceNumber, PreferencesWindowResourceNumber];

    public static IReadOnlyList<uint> ExplorationDestinationWindowResourceNumbers { get; } =
        [11500, 13500];

    public const uint HostileInteractionWindowResourceNumber = 3020;
    public const uint DialogueSpeechWindowResourceNumber = 12500;
    public const uint DialogueResponseWindowResourceNumber = 12501;
    public const uint FirstTyrDialogueScriptResourceNumber = 135;
    public const uint DialogueGlobalStringsScriptResourceNumber = 99;
    public const uint FirstTyrDialoguePortraitResourceNumber = 18;

    public static IReadOnlyList<uint> InteractionWindowResourceNumbers { get; } =
    [
        HostileInteractionWindowResourceNumber,
        DialogueSpeechWindowResourceNumber,
        DialogueResponseWindowResourceNumber
    ];

    public static IReadOnlyList<InteractionButtonAsset> InteractionButtonAssets { get; } =
    [
        new("talk", "images/interaction/talk.dsix", 15101, 16, 15, 4),
        new("use", "images/interaction/use.dsix", 15102, 16, 15, 4),
        new("pick-up", "images/interaction/pick-up.dsix", 15103, 16, 15, 4),
        new("talk-disabled", "images/interaction/talk-disabled.dsix", 15105, 16, 15, 4),
        new("use-disabled", "images/interaction/use-disabled.dsix", 15106, 16, 15, 4),
        new("pick-up-disabled", "images/interaction/pick-up-disabled.dsix", 15107, 16, 15, 4),
        new("dismiss", "images/interaction/dismiss.dsix", 15109, 28, 11, 1),
        new("dialogue-more", "images/dialogue/more.dsix", 12100, 14, 35, 4),
        new("dialogue-response-1", "images/dialogue/response-1.dsix", 12104, 302, 10, 1),
        new("dialogue-response-2", "images/dialogue/response-2.dsix", 12105, 302, 10, 1),
        new("dialogue-response-3", "images/dialogue/response-3.dsix", 12106, 302, 10, 1),
        new("dialogue-response-4", "images/dialogue/response-4.dsix", 12107, 302, 10, 1),
        new("dialogue-response-5", "images/dialogue/response-5.dsix", 12108, 302, 10, 1)
    ];

    public static IReadOnlyList<UiImageAsset> ExplorationCursorAssets { get; } =
    [
        new("walk", "images/cursors/walk.dsix", "ICON", 19101, 10, 13, 1),
        new("cannot-walk", "images/cursors/cannot-walk.dsix", "ICON", 19102, 16, 16, 1),
        new("melee-attack", "images/cursors/melee-attack.dsix", "ICON", 19103, 16, 17, 1),
        new("cannot-melee-attack", "images/cursors/cannot-melee-attack.dsix",
            "ICON", 19104, 16, 17, 1),
        new("ranged-attack", "images/cursors/ranged-attack.dsix", "ICON", 19105, 14, 15, 1),
        new("cannot-ranged-attack", "images/cursors/cannot-ranged-attack.dsix",
            "ICON", 19106, 16, 16, 1),
        new("look", "images/cursors/look.dsix", "ICON", 19107, 14, 15, 1),
        new("cannot-look", "images/cursors/cannot-look.dsix", "ICON", 19108, 16, 16, 1),
        new("cannot-cast", "images/cursors/cannot-cast.dsix", "ICON", 19109, 16, 17, 1),
        new("wait", "images/cursors/wait.dsix", "ICON", 19110, 13, 15, 1)
    ];

    public static UiLayerAsset GameMenuLayer { get; } =
        new("game-menu", "images/game-menu/base.dsix", 10000, 55, 42, 210, 116);

    public static UiLayerAsset InventoryLayer { get; } =
        new("inventory", "images/exploration/inventory-base.dsix", 13001, 0, 0, 320, 200);

    public static IReadOnlyList<UiLayerAsset> ExplorationDestinationTitleLayers { get; } =
    [
        new("effects-title", "images/exploration/effects-title.dsix",
            20075, 84, 0, 152, 23),
        new("use-title", "images/exploration/use-title.dsix",
            20080, 108, 0, 104, 23)
    ];

    public static IReadOnlyList<GameMenuButtonAsset> GameMenuButtons { get; } =
    [
        new("view-character", "images/game-menu/view-character.dsix",
            10300, 10100, 49, 24, 16, 16, 4),
        new("view-inventory", "images/game-menu/view-inventory.dsix",
            11304, 11102, 81, 24, 16, 16, 4),
        new("cast-spells-use-psionics", "images/game-menu/cast-spells-use-psionics.dsix",
            11305, 11103, 113, 24, 16, 16, 4),
        new("current-spell-effects", "images/game-menu/current-spell-effects.dsix",
            11306, 11104, 145, 24, 16, 16, 4),
        new("exit-to-dos", "images/game-menu/exit-to-dos.dsix",
            10301, 10101, 49, 51, 16, 16, 4),
        new("load-save", "images/game-menu/load-save.dsix",
            10302, 10102, 73, 51, 16, 16, 4),
        new("preferences", "images/game-menu/preferences.dsix",
            10303, 10103, 97, 51, 16, 16, 4),
        new("overhead-map", "images/game-menu/overhead-map.dsix",
            10305, 10105, 121, 51, 16, 16, 3),
        new("center-on-leader", "images/game-menu/center-on-leader.dsix",
            10306, 10106, 145, 51, 16, 16, 4),
        new("collapse-party", "images/game-menu/collapse-party.dsix",
            10313, 10113, 44, 78, 28, 16, 4),
        new("walk", "images/game-menu/walk.dsix",
            10310, 10110, 76, 78, 16, 16, 4),
        new("look", "images/game-menu/look.dsix",
            10311, 10111, 97, 78, 16, 16, 4),
        new("attack", "images/game-menu/attack.dsix",
            10312, 10112, 116, 78, 16, 16, 4),
        new("return-to-game", "images/game-menu/return-to-game.dsix",
            10308, 10108, 139, 78, 28, 16, 2)
    ];

    public static IReadOnlyList<GameMenuButtonAsset> PreferencesButtons { get; } =
    [
        new("music-on-off", "images/preferences/music-on-off.dsix",
            16300, 16100, 49, 24, 16, 16, 4),
        new("music-volume-increase", "images/preferences/increase.dsix",
            16304, 16104, 159, 29, 9, 8, 4),
        new("music-volume-decrease", "images/preferences/decrease.dsix",
            16305, 16105, 66, 29, 9, 8, 4),
        new("sound-effects-on-off", "images/preferences/sound-effects-on-off.dsix",
            16301, 16101, 49, 43, 16, 16, 4),
        new("sound-effects-volume-increase", "images/preferences/increase.dsix",
            16306, 16104, 159, 48, 9, 8, 4),
        new("sound-effects-volume-decrease", "images/preferences/decrease.dsix",
            16307, 16105, 66, 48, 9, 8, 4),
        new("animations-on-off", "images/preferences/animations-on-off.dsix",
            16302, 16102, 67, 78, 16, 16, 4),
        new("about", "images/preferences/about.dsix",
            16303, 16103, 49, 78, 16, 16, 4),
        new("difficulty-increase", "images/preferences/increase.dsix",
            16308, 16104, 149, 67, 9, 8, 4),
        new("difficulty-decrease", "images/preferences/decrease.dsix",
            16309, 16105, 56, 67, 9, 8, 4),
        new("game-menu", "images/preferences/game-menu.dsix",
            11308, 11101, 109, 78, 28, 16, 3),
        new("return-to-game", "images/game-menu/return-to-game.dsix",
            10308, 10108, 139, 78, 28, 16, 2),
        new("voice-effects-on-off", "images/preferences/voice-effects-on-off.dsix",
            16310, 16106, 85, 78, 16, 16, 4)
    ];

    public static IReadOnlyList<UiLayerAsset> StartMenuLayers { get; } =
    [
        new("stone-shell", "images/start-menu/stone-shell.dsix", 20029, 3, 44, 314, 112),
        new("flame-ornament", "images/start-menu/flame-ornament.dsix", 20028, 47, 24, 222, 33)
    ];

    public static IReadOnlyList<UiLayerAsset> PartyOverviewLayers { get; } =
    [
        new("party-overview-base", "images/party-overview/base.dsix", 11000, 0, 0, 320, 200),
        new("view-character-title", "images/party-overview/view-character-title.dsix",
            20079, 55, 0, 210, 23)
    ];

    public static IReadOnlyList<UiImageAsset> AddExistingCharacterAssets { get; } =
    [
        new("base", "images/add-existing/base.dsix", "BMP ", 10005, 320, 200, 1),
        new("title", "images/add-existing/title.dsix", "ICON", 18103, 104, 23, 1),
        new("add", "images/add-existing/add.dsix", "ICON", 18104, 44, 15, 4),
        new("delete", "images/add-existing/delete.dsix", "ICON", 18110, 62, 15, 4),
        new("row", "images/add-existing/row.dsix", "ICON", 18100, 165, 11, 4),
        new("scroll-up", "images/add-existing/scroll-up.dsix", "ICON", 12102, 14, 10, 4),
        new("scroll-down", "images/add-existing/scroll-down.dsix", "ICON", 12101, 14, 14, 4)
    ];

    public static IReadOnlyList<UiImagePlacement> AddExistingCharacterPlacements { get; } =
    [
        new("images/add-existing/base.dsix", 0, 0),
        new("images/add-existing/row.dsix", 46, 31),
        new("images/add-existing/row.dsix", 46, 42),
        new("images/add-existing/row.dsix", 46, 53),
        new("images/add-existing/row.dsix", 46, 64),
        new("images/add-existing/row.dsix", 46, 75),
        new("images/add-existing/row.dsix", 46, 86),
        new("images/add-existing/row.dsix", 46, 97),
        new("images/add-existing/row.dsix", 46, 108),
        new("images/add-existing/row.dsix", 46, 119),
        new("images/add-existing/row.dsix", 46, 130),
        new("images/add-existing/scroll-up.dsix", 215, 30),
        new("images/add-existing/scroll-down.dsix", 215, 130),
        new("images/add-existing/add.dsix", 231, 30),
        new("images/character-generation/exit.dsix", 231, 50),
        new("images/add-existing/delete.dsix", 215, 148),
        new("images/add-existing/title.dsix", 110, 0)
    ];

    public static IReadOnlyList<StartMenuButtonAsset> StartMenuButtons { get; } =
    [
        new("start-game", 19300, "images/start-menu/start-game.dsix", 19111, 127, 12, false),
        new("create-characters", 19301, "images/start-menu/create-characters.dsix", 19112, 220, 12, true),
        new("load-saved-game", 19302, "images/start-menu/load-saved-game.dsix", 19113, 191, 13, true),
        new("exit-to-dos", 19303, "images/start-menu/exit-to-dos.dsix", 19114, 127, 12, true)
    ];

    public static IReadOnlyList<CharacterGenerationButtonAsset> CharacterGenerationButtons { get; } =
    [
        new("cleric", "images/character-generation/cleric.dsix", 2002, 2002, 217, 10, 64, 6, 54, 7, 3),
        new("druid", "images/character-generation/druid.dsix", 2003, 2003, 217, 18, 56, 6, 50, 7, 3),
        new("fighter", "images/character-generation/fighter.dsix", 2004, 2004, 217, 26, 72, 6, 64, 7, 3),
        new("gladiator", "images/character-generation/gladiator.dsix", 2005, 2005, 217, 34, 88, 6, 79, 7, 3),
        new("preserver", "images/character-generation/preserver.dsix", 2006, 2006, 217, 42, 88, 6, 88, 7, 3),
        new("psionicist", "images/character-generation/psionicist.dsix", 2007, 2007, 217, 50, 94, 6, 78, 7, 3),
        new("ranger", "images/character-generation/ranger.dsix", 2008, 2008, 217, 58, 64, 6, 63, 7, 3),
        new("thief", "images/character-generation/thief.dsix", 2009, 2009, 217, 66, 56, 6, 48, 7, 3),
        new("exit", "images/character-generation/exit.dsix", 18302, 18109, 258, 154, 44, 15, 44, 15, 4),
        new("done", "images/character-generation/done.dsix", 19304, 19100, 243, 174, 59, 18, 59, 18, 4)
    ];

    public static IReadOnlyList<CharacterGenerationButtonAsset> CharacterGenerationModalButtons { get; } =
    [
        new("psionics", "images/character-generation/modal/psionics.dsix", 2038, 2038, 7, 15, 73, 7, 73, 7, 3),
        new("spells", "images/character-generation/modal/spells.dsix", 2039, 2039, 7, 23, 64, 7, 64, 7, 3),
        new("half-giants", "images/character-generation/modal/half-giants.dsix", 2040, 2040, 7, 31, 88, 7, 88, 7, 3),
        new("blank", "images/character-generation/modal/blank.dsix", 2041, 2041, 7, 39, 90, 7, 90, 7, 3),
        new("view-spheres", "images/character-generation/modal/view-spheres.dsix", 2046, 2046, 7, 47, 90, 7, 93, 7, 3),
        new("air", "images/character-generation/modal/air.dsix", 2042, 2042, 7, 15, 33, 7, 33, 7, 3),
        new("earth", "images/character-generation/modal/earth.dsix", 2043, 2043, 7, 23, 52, 7, 52, 7, 3),
        new("fire", "images/character-generation/modal/fire.dsix", 2044, 2044, 7, 31, 40, 7, 40, 7, 3),
        new("water", "images/character-generation/modal/water.dsix", 2045, 2045, 7, 39, 55, 7, 55, 7, 3),
        new("view-psionics", "images/character-generation/modal/view-psionics.dsix", 2047, 2047, 7, 47, 90, 7, 93, 7, 3)
    ];

    public static string DefaultAssetPackPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DarkSunWakeRedux", "UserContent");

    public static async Task<SourceIdentification> IdentifyAsync(
        string root,
        IEnumerable<SourceManifest> editions,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<ContentDiagnostic>();
        foreach (var edition in editions.OrderBy(candidate => candidate.SourceEdition, StringComparer.Ordinal))
        {
            var editionDiagnostics = await VerifySourceAsync(root, edition, cancellationToken);
            if (editionDiagnostics.Count == 0) return new(edition, []);
            diagnostics.AddRange(editionDiagnostics.Select(item => item with
            {
                Message = $"{edition.SourceEdition}: {item.Message}"
            }));
        }
        if (diagnostics.Count == 0)
            diagnostics.Add(new("source_editions_missing", "The Extractor contains no supported-edition manifests."));
        return new(null, diagnostics);
    }

    public static async Task<IReadOnlyList<ContentDiagnostic>> VerifySourceAsync(
        string root,
        SourceManifest manifest,
        CancellationToken cancellationToken = default)
    {
        manifest.Validate();
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return [new("source_root_missing", "The selected source directory does not exist.", root)];

        var diagnostics = new List<ContentDiagnostic>();
        foreach (var expected in manifest.Files)
        {
            var relative = SourceManifest.Normalize(expected.Path);
            var path = SafeTarget(root, relative);
            if (!File.Exists(path))
            {
                diagnostics.Add(new("source_file_missing", $"Required source file is missing: {relative}", relative));
                continue;
            }

            var actualSize = new FileInfo(path).Length;
            if (actualSize != expected.Size)
            {
                diagnostics.Add(new("source_size_mismatch", $"Source file has the wrong size: {relative}",
                    relative, expected.Size.ToString(), actualSize.ToString()));
                continue;
            }

            await using var stream = File.OpenRead(path);
            var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, cancellationToken));
            if (!hash.Equals(expected.Sha256, StringComparison.OrdinalIgnoreCase))
                diagnostics.Add(new("source_hash_mismatch", $"Source file has the wrong SHA-256: {relative}",
                    relative, expected.Sha256.ToLowerInvariant(), hash));
        }
        return diagnostics;
    }

    public static async Task<IReadOnlyList<ContentDiagnostic>> VerifyInstalledAsync(
        string root,
        CancellationToken cancellationToken = default)
    {
        var manifestPath = Path.Combine(root, "manifest.json");
        if (!File.Exists(manifestPath))
            return [new("pack_manifest_missing",
                $"Asset-pack manifest not found. Run DarkSunWakeRedux.Extractor against a supported GOG installation.",
                manifestPath)];

        AssetPackManifest? manifest;
        try
        {
            var info = new FileInfo(manifestPath);
            if (info.Length > MaximumManifestBytes)
                return [new("pack_manifest_too_large", "Asset-pack manifest exceeds the safety limit.",
                    "manifest.json", MaximumManifestBytes.ToString(), info.Length.ToString())];
            await using var stream = File.OpenRead(manifestPath);
            manifest = await JsonSerializer.DeserializeAsync<AssetPackManifest>(stream,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return [new("pack_manifest_unreadable", $"Asset-pack manifest cannot be read: {exception.Message}",
                "manifest.json")];
        }

        if (manifest is null) return [new("pack_manifest_empty", "Asset-pack manifest is empty.", "manifest.json")];
        var diagnostics = ValidatePackManifest(manifest);
        if (diagnostics.Count != 0) return diagnostics;

        var expectedPaths = new HashSet<string>(PathComparer);
        foreach (var asset in manifest.Files)
        {
            string path;
            try { path = SafeTarget(root, asset.Path); }
            catch (InvalidDataException)
            {
                diagnostics.Add(new("pack_path_unsafe", $"Asset path escapes the pack: {asset.Path}", asset.Path));
                continue;
            }
            expectedPaths.Add(path);
            if (!File.Exists(path))
            {
                diagnostics.Add(new("pack_asset_missing", $"Asset is missing: {asset.Path}", asset.Path));
                continue;
            }
            var actualSize = new FileInfo(path).Length;
            if (actualSize != asset.Size)
            {
                diagnostics.Add(new("pack_asset_size_mismatch", $"Asset has the wrong size: {asset.Path}",
                    asset.Path, asset.Size.ToString(), actualSize.ToString()));
                continue;
            }
            await using var stream = File.OpenRead(path);
            var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, cancellationToken));
            if (!hash.Equals(asset.Sha256, StringComparison.OrdinalIgnoreCase))
                diagnostics.Add(new("pack_asset_hash_mismatch", $"Asset has the wrong SHA-256: {asset.Path}",
                    asset.Path, asset.Sha256.ToLowerInvariant(), hash));
        }

        try
        {
            foreach (var installedPath in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                var fullPath = Path.GetFullPath(installedPath);
                if (PathComparer.Equals(fullPath, Path.GetFullPath(manifestPath))) continue;
                if (!expectedPaths.Contains(fullPath))
                    diagnostics.Add(new("pack_asset_unexpected", "Asset pack contains an unexpected file.",
                        Path.GetRelativePath(root, fullPath).Replace('\\', '/')));
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(new("pack_inventory_unreadable",
                $"Asset-pack inventory cannot be read: {exception.Message}"));
        }
        return diagnostics;
    }

    internal static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);

    private static List<ContentDiagnostic> ValidatePackManifest(AssetPackManifest manifest)
    {
        var diagnostics = new List<ContentDiagnostic>();
        if (manifest.FormatVersion != AssetPackFormatVersion)
            diagnostics.Add(new("pack_version_mismatch", "Asset-pack format version is incompatible.",
                "manifest.json", AssetPackFormatVersion.ToString(), manifest.FormatVersion.ToString()));
        if (!string.Equals(manifest.GameId, GameId, StringComparison.Ordinal))
            diagnostics.Add(new("pack_game_mismatch", "Asset pack belongs to a different game.", "manifest.json",
                GameId, manifest.GameId));
        if (string.IsNullOrWhiteSpace(manifest.SourceEdition))
            diagnostics.Add(new("pack_source_missing", "Asset-pack source edition is missing.", "manifest.json"));
        if (!IsSha256(manifest.SourceFingerprintSha256))
            diagnostics.Add(new("pack_source_hash_invalid", "Asset-pack source fingerprint is invalid.", "manifest.json"));
        if (string.IsNullOrWhiteSpace(manifest.ExtractorVersion))
            diagnostics.Add(new("pack_extractor_version_missing", "Extractor version is missing.", "manifest.json"));
        if (manifest.Files is null || manifest.Files.Count == 0)
        {
            diagnostics.Add(new("pack_inventory_empty", "Asset pack contains no files.", "manifest.json"));
            return diagnostics;
        }

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in manifest.Files)
        {
            if (asset is null || string.IsNullOrWhiteSpace(asset.Path))
            {
                diagnostics.Add(new("pack_path_missing", "Asset path is missing.", "manifest.json"));
                continue;
            }
            string normalized;
            try { normalized = SourceManifest.Normalize(asset.Path); }
            catch (InvalidDataException)
            {
                diagnostics.Add(new("pack_path_unsafe", $"Asset path is unsafe: {asset.Path}", asset.Path));
                continue;
            }
            if (!paths.Add(normalized))
                diagnostics.Add(new("pack_path_duplicate", $"Asset path is duplicated: {normalized}", normalized));
            if (asset.Size < 0 || !IsSha256(asset.Sha256))
                diagnostics.Add(new("pack_fingerprint_invalid", $"Asset fingerprint is invalid: {normalized}", normalized));
            if (string.IsNullOrWhiteSpace(asset.SourcePath))
                diagnostics.Add(new("pack_provenance_missing", $"Asset source path is missing: {normalized}", normalized));
            if (string.IsNullOrWhiteSpace(asset.MediaType) || string.IsNullOrWhiteSpace(asset.Conversion))
                diagnostics.Add(new("pack_conversion_missing", $"Asset media type or conversion is missing: {normalized}", normalized));
        }
        return diagnostics;
    }

    private static string SafeTarget(string root, string relative)
    {
        if (Path.IsPathFullyQualified(relative)) throw new InvalidDataException("Path must be relative.");
        var normalized = SourceManifest.Normalize(relative).Replace('/', Path.DirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(root).TrimEnd(
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var target = Path.GetFullPath(Path.Combine(root, normalized));
        if (!target.StartsWith(fullRoot, OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidDataException("Path escapes content root.");
        return target;
    }

    private static StringComparer PathComparer => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
}
