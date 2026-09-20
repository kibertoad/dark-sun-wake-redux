using System.Buffers.Binary;

namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Summarizes whether one aligned little-endian word in each opaque record
/// lane belongs to a caller-supplied resource-number namespace. This preserves
/// structural evidence without assigning a field or record meaning.
/// </summary>
public sealed record OpaqueLaneWordNamespaceProfile(
    int ResourceByteLength,
    int RecordWidth,
    int RecordCount,
    int LaneWidth,
    int WordOffset,
    int CandidateWordCount,
    int NamespaceMemberCount,
    int DistinctCandidateValueCount,
    int DistinctNamespaceMemberCount,
    int RecordsWithRepeatedCandidateValues);

public static class OpaqueLaneWordNamespaceProfiler
{
    public const int MaximumResourceBytes = 1_048_576;
    public const int MaximumRecordWidth = 4_096;
    public const int MaximumRecordCount = 65_536;

    public static OpaqueLaneWordNamespaceProfile Create(
        ReadOnlyMemory<byte> payload,
        int recordWidth,
        int laneWidth,
        int wordOffset,
        IReadOnlySet<uint> targetResourceNumbers,
        string sourceName = "opaque lane payload")
    {
        ArgumentNullException.ThrowIfNull(targetResourceNumbers);
        if (recordWidth is < 1 or > MaximumRecordWidth)
            throw new ArgumentOutOfRangeException(nameof(recordWidth), recordWidth,
                $"Record width must be between 1 and {MaximumRecordWidth} bytes.");
        if (laneWidth < sizeof(ushort) || laneWidth > recordWidth ||
            recordWidth % laneWidth != 0)
            throw new ArgumentOutOfRangeException(nameof(laneWidth), laneWidth,
                "Lane width must divide the record width and contain at least one word.");
        if (wordOffset < 0 || wordOffset > laneWidth - sizeof(ushort))
            throw new ArgumentOutOfRangeException(nameof(wordOffset), wordOffset,
                "Word offset must identify a complete word within each lane.");
        if (payload.Length is 0 or > MaximumResourceBytes)
            throw Error(sourceName, $"has invalid length {payload.Length} bytes");
        if (payload.Length % recordWidth != 0)
            throw Error(sourceName, $"length {payload.Length} is not divisible by record width {recordWidth}");
        if (targetResourceNumbers.Count == 0)
            throw Error(sourceName, "cannot be compared with an empty target resource namespace");

        var recordCount = payload.Length / recordWidth;
        if (recordCount > MaximumRecordCount)
            throw Error(sourceName, $"declares more than {MaximumRecordCount} records");

        var laneCount = recordWidth / laneWidth;
        var candidates = new HashSet<ushort>();
        var namespaceMembers = new HashSet<ushort>();
        var repeatedRows = 0;
        var matchedCount = 0;
        var bytes = payload.Span;
        for (var record = 0; record < recordCount; record++)
        {
            var rowValues = new HashSet<ushort>();
            var recordStart = record * recordWidth;
            for (var lane = 0; lane < laneCount; lane++)
            {
                var value = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(
                    recordStart + lane * laneWidth + wordOffset, sizeof(ushort)));
                candidates.Add(value);
                rowValues.Add(value);
                if (targetResourceNumbers.Contains(value))
                {
                    matchedCount++;
                    namespaceMembers.Add(value);
                }
            }
            if (rowValues.Count != laneCount) repeatedRows++;
        }

        return new(payload.Length, recordWidth, recordCount, laneWidth, wordOffset,
            checked(recordCount * laneCount), matchedCount, candidates.Count,
            namespaceMembers.Count, repeatedRows);
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
