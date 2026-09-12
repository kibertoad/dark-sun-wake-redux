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

public sealed record SourceIdentification(
    SourceManifest? Edition,
    IReadOnlyList<ContentDiagnostic> Diagnostics)
{
    public bool IsSupported => Edition is not null;
}

public static class OriginalContent
{
    public const int AssetPackFormatVersion = 8;
    public const string GameId = "dark-sun-wake-redux";
    public const string TitleImageAssetPath = "images/title.dsix";
    public const string InterfaceFontAssetPath = "fonts/interface.dsft";
    public const string TextCatalogAssetPath = "text/resources.dstx";
    public const string PartyWindowImageAssetPath = "images/party/window-image.dsix";
    public const string StartFlowUiCatalogAssetPath = "ui/start-flow.dsui";
    public const long MaximumManifestBytes = 4 * 1024 * 1024;

    public static IReadOnlyList<uint> StartFlowWindowResourceNumbers { get; } =
        [19500, 19501, 19502, 19503, 19504, 19505];

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
