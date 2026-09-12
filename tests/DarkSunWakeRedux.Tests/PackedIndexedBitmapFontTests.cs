using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedIndexedBitmapFontTests
{
    [Fact]
    public void RoundTripsCharacterMapAndGlyphPixels()
    {
        var packed = Font();
        using var stream = new MemoryStream();
        packed.Write(stream);
        stream.Position = 0;

        var decoded = PackedIndexedBitmapFont.Read(stream, "synthetic.dsft");

        Assert.Equal(packed.CharacterMap, decoded.CharacterMap);
        Assert.Equal(3, decoded.Glyphs[65].Width);
        Assert.Equal(2, decoded.Glyphs[65].Height);
        Assert.Equal([1, 2, 3, 4, 5, 6], decoded.Glyphs[65].Pixels);
    }

    [Fact]
    public void RejectsSignatureVersionTruncationAndTrailingData()
    {
        var bytes = Bytes(Font());
        bytes[0] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => Read(bytes));

        bytes = Bytes(Font());
        bytes[4]++;
        Assert.Throws<InvalidDataException>(() => Read(bytes));

        bytes = Bytes(Font());
        Assert.Throws<InvalidDataException>(() => Read(bytes[..^1]));

        bytes = [.. Bytes(Font()), 0];
        Assert.Throws<InvalidDataException>(() => Read(bytes));
    }

    [Fact]
    public void RejectsInconsistentGlyphDimensionsWhenWriting()
    {
        var packed = Font();
        var glyphs = packed.Glyphs.ToArray();
        glyphs[1] = new(1, 3, [1, 2, 3]);

        Assert.Throws<InvalidDataException>(() =>
            new PackedIndexedBitmapFont(packed.CharacterMap, glyphs).Write(new MemoryStream()));
    }

    private static PackedIndexedBitmapFont Font()
    {
        var map = Enumerable.Range(0, IndexedBitmapFont.CharacterCount).Select(value => (byte)value).ToArray();
        var glyphs = Enumerable.Range(0, IndexedBitmapFont.CharacterCount)
            .Select(_ => new IndexedFontGlyph(0, 2, []))
            .ToArray();
        glyphs[65] = new(3, 2, [1, 2, 3, 4, 5, 6]);
        return new(map, glyphs);
    }

    private static byte[] Bytes(PackedIndexedBitmapFont font)
    {
        using var stream = new MemoryStream();
        font.Write(stream);
        return stream.ToArray();
    }

    private static PackedIndexedBitmapFont Read(byte[] bytes) =>
        PackedIndexedBitmapFont.Read(new MemoryStream(bytes), "broken.dsft");
}
