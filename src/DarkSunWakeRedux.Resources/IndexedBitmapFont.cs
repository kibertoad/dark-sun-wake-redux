using System.Buffers.Binary;

namespace DarkSunWakeRedux.Resources;

public sealed record IndexedFontGlyph(int Width, int Height, byte[] Pixels);

public sealed record IndexedBitmapFont(
    IReadOnlyList<byte> CharacterMap,
    IReadOnlyList<IndexedFontGlyph> Glyphs)
{
    public const int CharacterCount = 256;
    public const int HeaderSize = 8 + CharacterCount + CharacterCount * 2;
    public const int MaximumPayloadBytes = 1024 * 1024;
    public const int MaximumGlyphWidth = 256;
    public const int MaximumGlyphHeight = 64;
    public const int MaximumPixels = CharacterCount * MaximumGlyphWidth * MaximumGlyphHeight;

    public static IndexedBitmapFont Read(ReadOnlyMemory<byte> payload, string sourceName = "FONT resource")
    {
        if (payload.Length < HeaderSize)
            throw Error(sourceName, $"is shorter than the {HeaderSize}-byte font header");
        if (payload.Length > MaximumPayloadBytes)
            throw Error(sourceName, $"exceeds the {MaximumPayloadBytes}-byte safety limit");
        var bytes = payload.ToArray();
        var glyphCount = UInt16(bytes, 0, sourceName, "glyph count");
        if (glyphCount != CharacterCount)
            throw Error(sourceName, $"declares {glyphCount} glyphs; expected {CharacterCount}");
        var height = UInt16(bytes, 2, sourceName, "glyph height");
        if (height is 0 or > MaximumGlyphHeight)
            throw Error(sourceName, $"declares invalid glyph height {height}");

        var characterMap = bytes.AsSpan(8, CharacterCount).ToArray();
        var offsets = new int[CharacterCount];
        for (var index = 0; index < offsets.Length; index++)
        {
            var offset = UInt16(bytes, 8 + CharacterCount + index * 2, sourceName, $"glyph {index} offset");
            if (offset < HeaderSize || offset > bytes.Length - 2)
                throw Error(sourceName, $"glyph {index} has out-of-range offset {offset}");
            offsets[index] = offset;
            if (index > 0 && offsets[index] <= offsets[index - 1])
                throw Error(sourceName, "glyph offsets are not strictly increasing");
        }

        var glyphs = new IndexedFontGlyph[CharacterCount];
        var totalPixels = 0;
        for (var index = 0; index < glyphs.Length; index++)
        {
            var start = offsets[index];
            var end = index + 1 < offsets.Length ? offsets[index + 1] : bytes.Length;
            var width = UInt16(bytes, start, sourceName, $"glyph {index} width");
            if (width > MaximumGlyphWidth)
                throw Error(sourceName, $"glyph {index} width {width} exceeds the safety limit");
            var pixelCount = checked(width * height);
            totalPixels = checked(totalPixels + pixelCount);
            if (totalPixels > MaximumPixels)
                throw Error(sourceName, "decoded glyph pixels exceed the safety limit");
            var expectedEnd = checked(start + 2 + pixelCount);
            if (expectedEnd != end)
                throw Error(sourceName,
                    $"glyph {index} record ends at {end}, but its {width}x{height} dimensions require {expectedEnd}");
            glyphs[index] = new(width, height, bytes.AsSpan(start + 2, pixelCount).ToArray());
        }
        return new(characterMap, glyphs);
    }

    private static ushort UInt16(byte[] bytes, int offset, string sourceName, string field)
    {
        if (offset < 0 || offset + 2 > bytes.Length)
            throw Error(sourceName, $"is truncated while reading {field} at offset {offset}");
        return BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
