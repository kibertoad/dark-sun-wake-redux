using System.Text.Json;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

/// <summary>
/// The rebuild's own options, which the launch options screen sets (DEV-UI-001). Each field is
/// the setting of one deviation; a later option gets its own field and default.
/// </summary>
public sealed record LaunchSettings(bool WideMapView)
{
    /// <summary>Wide map view is DEV-EXPLORE-001, on by default.</summary>
    public static LaunchSettings Default { get; } = new(WideMapView: true);
}

public sealed record LoadedLaunchSettings(
    LaunchSettings Settings,
    bool RecoveredFromBackup,
    bool UsedDefaults);

/// <summary>
/// Stores <see cref="LaunchSettings"/> in a versioned, size-bounded JSON file. Writes go to a
/// temporary file that replaces the target in one rename, after the last readable copy is kept as
/// <c>settings.json.bak</c>. Reads take each field from the file, then from the backup, then from
/// the defaults, and never throw for anything the files contain.
/// </summary>
public static class LaunchSettingsStore
{
    public const int CurrentVersion = 1;
    public const int MaximumBytes = 4096;
    public const string FileName = "settings.json";
    public const string BackupSuffix = ".bak";
    private const string VersionField = "version";
    private const string WideMapViewField = "wideMapView";

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        MaxDepth = 4,
        CommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    private static readonly JsonWriterOptions WriterOptions = new() { Indented = true };

    /// <summary>The per-user data folder that also holds <c>UserContent</c>.</summary>
    public static string DefaultPath() => Path.Combine(
        Path.GetDirectoryName(OriginalContent.DefaultAssetPackPath())
            ?? throw new InvalidOperationException("The asset pack path has no parent folder."),
        FileName);

    public static LoadedLaunchSettings ReadOrDefault(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var target = Path.GetFullPath(path);
        using var primary = TryParse(target);
        using var backup = TryParse(target + BackupSuffix);
        var field = RefurbishedDinosaurs.Core.Persistence.SettingsRecovery.Select(
            ReadBoolean(primary, WideMapViewField), ReadBoolean(backup, WideMapViewField),
            LaunchSettings.Default.WideMapView, _ => true);
        return new(new(field.Value), field.Source == RefurbishedDinosaurs.Core.Persistence.SettingsSource.Backup,
            field.Source == RefurbishedDinosaurs.Core.Persistence.SettingsSource.Default);
    }

    public static void WriteAtomic(string path, LaunchSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(settings);
        var bytes = Serialize(settings);
        RefurbishedDinosaurs.Core.Persistence.RecoverableFile.Write(path, stream => stream.Write(bytes), candidate =>
        {
            using var parsed = TryParse(candidate);
            if (parsed is null) throw new InvalidDataException("Settings generation is invalid.");
        }, error => error is IOException or InvalidDataException or UnauthorizedAccessException);
    }

    public static byte[] Serialize(LaunchSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteNumber(VersionField, CurrentVersion);
            writer.WriteBoolean(WideMapViewField, settings.WideMapView);
            writer.WriteEndObject();
        }
        if (buffer.Length > MaximumBytes)
            throw new InvalidDataException(
                $"The settings document exceeds its {MaximumBytes}-byte limit.");
        return buffer.ToArray();
    }

    /// <summary>
    /// Returns the file's root object when the file exists, fits the size limit, parses, and
    /// carries a version this build knows; otherwise null.
    /// </summary>
    private static JsonDocument? TryParse(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var bytes = RefurbishedDinosaurs.Core.Persistence.RecoverableFile.ReadBounded(path, MaximumBytes);
            var document = JsonDocument.Parse(bytes, DocumentOptions);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty(VersionField, out var version) &&
                version.ValueKind == JsonValueKind.Number &&
                version.TryGetInt32(out var number) &&
                number is >= 1 and <= CurrentVersion)
                return document;
            document.Dispose();
            return null;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or
            UnauthorizedAccessException or JsonException or NotSupportedException or
            System.Security.SecurityException or ArgumentException)
        {
            return null;
        }
    }

    private static bool? ReadBoolean(JsonDocument? document, string field) =>
        document is not null &&
        document.RootElement.TryGetProperty(field, out var value) &&
        value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
}
