namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Summarizes fixed-width opaque records without exposing or assigning a meaning
/// to their source bytes. It is intended to test a candidate structural stride
/// before a format reader is proposed.
/// </summary>
public sealed record OpaqueRecordColumnProfile(
    int Offset,
    int DistinctValueCount,
    int ZeroValueCount,
    byte MinimumValue,
    byte MaximumValue);

public sealed record OpaqueRecordProfile(
    int ResourceByteLength,
    int RecordWidth,
    int RecordCount,
    IReadOnlyList<OpaqueRecordColumnProfile> Columns)
{
    public const int MaximumResourceBytes = 1_048_576;
    public const int MaximumRecordWidth = 4_096;
    public const int MaximumRecordCount = 65_536;

    public static OpaqueRecordProfile Create(
        ReadOnlyMemory<byte> payload,
        int recordWidth,
        string sourceName = "opaque record payload")
    {
        if (recordWidth is < 1 or > MaximumRecordWidth)
            throw new ArgumentOutOfRangeException(nameof(recordWidth), recordWidth,
                $"Record width must be between 1 and {MaximumRecordWidth} bytes.");
        if (payload.Length is 0 or > MaximumResourceBytes)
            throw Error(sourceName, $"has invalid length {payload.Length} bytes");
        if (payload.Length % recordWidth != 0)
            throw Error(sourceName, $"length {payload.Length} is not divisible by candidate " +
                $"record width {recordWidth}");
        var recordCount = payload.Length / recordWidth;
        if (recordCount > MaximumRecordCount)
            throw Error(sourceName, $"declares more than {MaximumRecordCount} candidate records");

        var columns = new OpaqueRecordColumnProfile[recordWidth];
        var bytes = payload.Span;
        Span<bool> observed = stackalloc bool[byte.MaxValue + 1];
        for (var column = 0; column < recordWidth; column++)
        {
            observed.Clear();
            var distinct = 0;
            var zeroes = 0;
            var minimum = byte.MaxValue;
            var maximum = byte.MinValue;
            for (var record = 0; record < recordCount; record++)
            {
                var value = bytes[record * recordWidth + column];
                if (!observed[value])
                {
                    observed[value] = true;
                    distinct++;
                }
                if (value == 0) zeroes++;
                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
            }
            columns[column] = new(column, distinct, zeroes, minimum, maximum);
        }
        return new(payload.Length, recordWidth, recordCount, columns);
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
