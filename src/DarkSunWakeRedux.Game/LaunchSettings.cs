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
        var recovered = false;
        var defaulted = false;
        var wideMapView = ReadBoolean(primary, WideMapViewField)
            ?? Recover(ReadBoolean(backup, WideMapViewField))
            ?? UseDefault(LaunchSettings.Default.WideMapView);
        return new(new(wideMapView), recovered, defaulted);

        T? Recover<T>(T? value) where T : struct
        {
            recovered |= value.HasValue;
            return value;
        }

        T UseDefault<T>(T value)
        {
            defaulted = true;
            return value;
        }
    }

    public static void WriteAtomic(string path, LaunchSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(settings);
        var bytes = Serialize(settings);
        var target = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(target)
            ?? throw new ArgumentException("The settings path needs a parent folder.", nameof(path));
        Directory.CreateDirectory(directory);
        var temporary = target + $".{Guid.NewGuid():N}.tmp";
        var backupTemporary = target + BackupSuffix + $".{Guid.NewGuid():N}.tmp";
        try
        {
            WriteDurably(temporary, bytes);
            // Only a copy that reads back becomes the backup, so a damaged file never replaces a
            // good backup. A file with a version this build does not know is dropped with no copy
            // kept. The primary stays in place until the final rename replaces it.
            using (var current = TryParse(target))
            {
                if (current is not null)
                {
                    File.Copy(target, backupTemporary, overwrite: false);
                    File.Move(backupTemporary, target + BackupSuffix, overwrite: true);
                }
            }
            File.Move(temporary, target, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            if (File.Exists(backupTemporary)) File.Delete(backupTemporary);
        }
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

    private static void WriteDurably(string path, byte[] bytes)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
            FileShare.None, bufferSize: 4096, FileOptions.WriteThrough);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
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
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length is <= 0 or > MaximumBytes) return null;
            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes);
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
        catch (Exception exception) when (exception is IOException or
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
