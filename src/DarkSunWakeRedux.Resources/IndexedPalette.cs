namespace DarkSunWakeRedux.Resources;

public readonly record struct Rgb24(byte Red, byte Green, byte Blue);

public sealed record IndexedPalette(IReadOnlyList<Rgb24> Colors)
{
    public const int ColorCount = 256;
    public const int EncodedLength = ColorCount * 3;

    public static IndexedPalette Read(ReadOnlySpan<byte> payload, string sourceName = "palette resource")
    {
        if (payload.Length != EncodedLength)
            throw new InvalidDataException(
                $"{sourceName}: palette must contain exactly {EncodedLength} bytes, found {payload.Length}.");
        var colors = new Rgb24[ColorCount];
        for (var index = 0; index < ColorCount; index++)
        {
            var offset = index * 3;
            var red = payload[offset];
            var green = payload[offset + 1];
            var blue = payload[offset + 2];
            if (red > 63 || green > 63 || blue > 63)
                throw new InvalidDataException(
                    $"{sourceName}: color {index} contains a component outside the 6-bit VGA range.");
            colors[index] = new((byte)(red * 4), (byte)(green * 4), (byte)(blue * 4));
        }
        return new(colors);
    }
}
