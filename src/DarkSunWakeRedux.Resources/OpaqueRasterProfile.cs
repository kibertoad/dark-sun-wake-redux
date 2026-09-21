namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Summarizes a candidate rectangular opaque byte stream without retaining or
/// assigning a meaning to the source bytes. The profile can distinguish a
/// plausible raster arrangement from a proposed fixed-width record layout,
/// but it is not an image decoder or a format claim.
/// </summary>
public sealed record OpaqueRasterProfile(
    int ResourceByteLength,
    int CandidateWidth,
    int CandidateHeight,
    int DistinctValueCount,
    int ZeroValueCount,
    int HorizontalAdjacentPairCount,
    int HorizontalEqualPairCount,
    int VerticalAdjacentPairCount,
    int VerticalEqualPairCount)
{
    public const int MaximumResourceBytes = 1_048_576;
    public const int MaximumWidth = 4_096;
    public const int MaximumHeight = 65_536;

    public static OpaqueRasterProfile Create(
        ReadOnlyMemory<byte> payload,
        int candidateWidth,
        string sourceName = "opaque raster payload")
    {
        if (candidateWidth is < 1 or > MaximumWidth)
            throw new ArgumentOutOfRangeException(nameof(candidateWidth), candidateWidth,
                $"Candidate width must be between 1 and {MaximumWidth} bytes.");
        if (payload.Length is 0 or > MaximumResourceBytes)
            throw Error(sourceName, $"has invalid length {payload.Length} bytes");
        if (payload.Length % candidateWidth != 0)
            throw Error(sourceName, $"length {payload.Length} is not divisible by candidate " +
                $"width {candidateWidth}");

        var candidateHeight = payload.Length / candidateWidth;
        if (candidateHeight > MaximumHeight)
            throw Error(sourceName, $"declares more than {MaximumHeight} candidate rows");

        var bytes = payload.Span;
        Span<bool> observed = stackalloc bool[byte.MaxValue + 1];
        var distinct = 0;
        var zeroes = 0;
        var horizontalPairs = 0;
        var horizontalEqualPairs = 0;
        var verticalPairs = 0;
        var verticalEqualPairs = 0;

        for (var row = 0; row < candidateHeight; row++)
        {
            var rowStart = row * candidateWidth;
            for (var column = 0; column < candidateWidth; column++)
            {
                var offset = rowStart + column;
                var value = bytes[offset];
                if (!observed[value])
                {
                    observed[value] = true;
                    distinct++;
                }
                if (value == 0) zeroes++;
                if (column > 0)
                {
                    horizontalPairs++;
                    if (value == bytes[offset - 1]) horizontalEqualPairs++;
                }
                if (row > 0)
                {
                    verticalPairs++;
                    if (value == bytes[offset - candidateWidth]) verticalEqualPairs++;
                }
            }
        }

        return new(payload.Length, candidateWidth, candidateHeight, distinct, zeroes,
            horizontalPairs, horizontalEqualPairs, verticalPairs, verticalEqualPairs);
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
