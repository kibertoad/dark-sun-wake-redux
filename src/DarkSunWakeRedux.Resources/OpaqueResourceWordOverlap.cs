namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Tests an opaque payload's unaligned little-endian 16-bit windows against a
/// caller-supplied resource-number namespace without retaining source values
/// or assigning a record layout.
/// </summary>
public sealed record OpaqueResourceWordOverlap(
    int SourceByteLength,
    int CandidateWordCount,
    IReadOnlyList<int> MatchingOffsets)
{
    public const int MaximumSourceBytes = 1_048_576;

    public static OpaqueResourceWordOverlap FindLittleEndianMatches(
        ReadOnlyMemory<byte> payload,
        IReadOnlySet<uint> targetResourceNumbers,
        string sourceName = "opaque resource payload")
    {
        ArgumentNullException.ThrowIfNull(targetResourceNumbers);
        if (payload.Length > MaximumSourceBytes)
            throw Error(sourceName, $"exceeds {MaximumSourceBytes} bytes");
        if (targetResourceNumbers.Count == 0)
            throw Error(sourceName, "cannot be compared with an empty target resource namespace");

        var bytes = payload.Span;
        var matches = new List<int>();
        for (var offset = 0; offset + 1 < bytes.Length; offset++)
        {
            var value = (uint)(bytes[offset] | (bytes[offset + 1] << 8));
            if (targetResourceNumbers.Contains(value)) matches.Add(offset);
        }

        return new(payload.Length, Math.Max(0, payload.Length - 1), matches);
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
