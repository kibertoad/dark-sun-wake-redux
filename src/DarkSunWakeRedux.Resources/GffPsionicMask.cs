namespace DarkSunWakeRedux.Resources;

public sealed record GffPsionicMask(byte RawMask)
{
    public const byte DefinedBits = 0b0000_0111;

    public static GffPsionicMask Read(ReadOnlyMemory<byte> data, string sourceName)
    {
        if (data.Length != 1)
            throw new InvalidDataException(
                $"{sourceName}: PSIN resource must contain exactly one byte; found {data.Length}.");

        var mask = data.Span[0];
        // PLACEHOLDER: FMT-PARTY-003 - what the byte means is unknown; only the shipped range is accepted.
        if (mask == 0 || (mask & ~DefinedBits) != 0)
            throw new InvalidDataException(
                $"{sourceName}: PSIN mask 0x{mask:x2} is outside the nonzero three-bit envelope.");

        return new(mask);
    }
}
