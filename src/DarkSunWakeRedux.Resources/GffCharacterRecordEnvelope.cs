namespace DarkSunWakeRedux.Resources;

public sealed record GffCharacterRecordEnvelope(byte TailRecordCount)
{
    public const byte SupportedVersion = 1;
    public const int FixedHeaderSize = 79;
    public const int TailRecordSize = 33;
    public const int MaximumResourceSize = FixedHeaderSize + byte.MaxValue * TailRecordSize;

    public static GffCharacterRecordEnvelope Read(ReadOnlyMemory<byte> data, string sourceName)
    {
        if (data.Length < FixedHeaderSize)
            throw new InvalidDataException(
                $"{sourceName}: CHAR resource is shorter than its {FixedHeaderSize}-byte fixed header.");
        if (data.Length > MaximumResourceSize)
            throw new InvalidDataException(
                $"{sourceName}: CHAR resource exceeds the {MaximumResourceSize}-byte structural maximum.");
        var version = data.Span[0];
        if (version != SupportedVersion)
            throw new InvalidDataException(
                $"{sourceName}: CHAR resource uses unsupported version {version}.");
        var count = data.Span[1];
        var expectedSize = FixedHeaderSize + count * TailRecordSize;
        if (data.Length != expectedSize)
            throw new InvalidDataException(
                $"{sourceName}: CHAR resource declares {count} tail records and must contain " +
                $"{expectedSize} bytes; found {data.Length}.");
        return new(count);
    }
}
