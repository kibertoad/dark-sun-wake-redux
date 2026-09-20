using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record GffResourcePatternMatch(
    string Tag,
    uint ResourceNumber,
    uint ResourceSize,
    int PatternOffset);

/// <summary>
/// Locates a supplied printable ASCII pattern in already-validated GFF resources
/// while returning only descriptor metadata and an offset within the resource.
/// </summary>
public static class GffResourcePatternLocator
{
    public const int MaximumPatternLength = 128;
    public const int MaximumMatches = 1_024;

    public static IReadOnlyList<GffResourcePatternMatch> FindAscii(
        GffArchive archive,
        string pattern)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        if (pattern.Length > MaximumPatternLength ||
            pattern.Any(character => character is < ' ' or > '~'))
            throw new ArgumentException(
                $"Pattern must contain 1-{MaximumPatternLength} printable ASCII characters.",
                nameof(pattern));

        var bytes = Encoding.ASCII.GetBytes(pattern);
        var matches = new List<GffResourcePatternMatch>();
        foreach (var resource in archive.Resources)
        {
            if (resource.Size < bytes.Length) continue;
            var offset = archive.GetResource(resource.Tag, resource.Number)
                .Span.IndexOf(bytes);
            if (offset < 0) continue;
            if (matches.Count == MaximumMatches)
                throw new InvalidDataException(
                    $"Pattern matches more than the {MaximumMatches}-resource safety limit.");
            matches.Add(new(resource.Tag, resource.Number, resource.Size, offset));
        }
        return matches.AsReadOnly();
    }
}
