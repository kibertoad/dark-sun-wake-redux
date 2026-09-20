namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Compares the four structurally exposed OJFF words with a caller-supplied
/// resource-number subset without assigning a field meaning.
/// </summary>
public sealed record ObjectRawWordPatternMatch(int Offset, bool MatchesPatternResource);

public static class ObjectResourcePatternOverlap
{
    public static IReadOnlyList<ObjectRawWordPatternMatch> Match(
        ushort rawWord0,
        ushort rawWord6,
        ushort rawWord8,
        ushort rawWord10,
        IReadOnlySet<uint> resourceNumbers)
    {
        ArgumentNullException.ThrowIfNull(resourceNumbers);
        return Array.AsReadOnly<ObjectRawWordPatternMatch>(
        [
            new(0, resourceNumbers.Contains(rawWord0)),
            new(6, resourceNumbers.Contains(rawWord6)),
            new(8, resourceNumbers.Contains(rawWord8)),
            new(10, resourceNumbers.Contains(rawWord10))
        ]);
    }
}
