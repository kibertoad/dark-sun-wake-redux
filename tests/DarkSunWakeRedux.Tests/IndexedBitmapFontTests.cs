using System.Buffers.Binary;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class IndexedBitmapFontTests
{
    [Fact]
    public void ReadsCharacterMapAndIndexedGlyphPixels()
    {
        var bytes = FontBytes(height: 2, specialGlyph: 65, width: 2, pixels: [0, 20, 254, 0]);

        var font = IndexedBitmapFont.Read(bytes, "synthetic FONT #100");

        Assert.Equal(IndexedBitmapFont.CharacterCount, font.Glyphs.Count);
        Assert.Equal((byte)65, font.CharacterMap[65]);
        Assert.Equal(2, font.Glyphs[65].Width);
        Assert.Equal(2, font.Glyphs[65].Height);
        Assert.Equal([0, 20, 254, 0], font.Glyphs[65].Pixels);
    }

    [Fact]
    public void AllowsZeroWidthControlGlyphs()
    {
        var font = IndexedBitmapFont.Read(FontBytes(), "controls.font");

        Assert.Equal(0, font.Glyphs[0].Width);
        Assert.Empty(font.Glyphs[0].Pixels);
    }

    [Fact]
    public void RejectsWrongGlyphCountAndHeight()
    {
        var count = FontBytes();
        BinaryPrimitives.WriteUInt16LittleEndian(count.AsSpan(0, 2), 255);
        Assert.Throws<InvalidDataException>(() => IndexedBitmapFont.Read(count, "count.font"));

        var height = FontBytes();
        BinaryPrimitives.WriteUInt16LittleEndian(height.AsSpan(2, 2), 0);
        Assert.Throws<InvalidDataException>(() => IndexedBitmapFont.Read(height, "height.font"));
    }

    [Fact]
    public void RejectsOutOfRangeAndNonIncreasingOffsets()
    {
        var outside = FontBytes();
        BinaryPrimitives.WriteUInt16LittleEndian(outside.AsSpan(264, 2), 0);
        Assert.Throws<InvalidDataException>(() => IndexedBitmapFont.Read(outside, "outside.font"));

        var duplicate = FontBytes();
        duplicate.AsSpan(264, 2).CopyTo(duplicate.AsSpan(266, 2));
        Assert.Throws<InvalidDataException>(() => IndexedBitmapFont.Read(duplicate, "duplicate.font"));
    }

    [Fact]
    public void RejectsGlyphRecordThatDoesNotMatchDimensions()
    {
        var bytes = FontBytes(height: 2, specialGlyph: 65, width: 2, pixels: [1, 2, 3, 4]);
        var glyphOffset = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(264 + 65 * 2, 2));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(glyphOffset, 2), 3);

        var exception = Assert.Throws<InvalidDataException>(() =>
            IndexedBitmapFont.Read(bytes, "mismatch.font"));

        Assert.Contains("dimensions require", exception.Message, StringComparison.Ordinal);
    }

    private static byte[] FontBytes(
        ushort height = 2,
        int specialGlyph = -1,
        ushort width = 0,
        byte[]? pixels = null)
    {
        var widths = new ushort[IndexedBitmapFont.CharacterCount];
        if (specialGlyph >= 0) widths[specialGlyph] = width;
        var size = IndexedBitmapFont.HeaderSize + widths.Sum(value => 2 + value * height);
        var bytes = new byte[size];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0, 2), IndexedBitmapFont.CharacterCount);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2, 2), height);
        for (var index = 0; index < IndexedBitmapFont.CharacterCount; index++) bytes[8 + index] = (byte)index;
        var offset = IndexedBitmapFont.HeaderSize;
        for (var index = 0; index < widths.Length; index++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(264 + index * 2, 2), checked((ushort)offset));
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset, 2), widths[index]);
            if (index == specialGlyph && pixels is not null) pixels.CopyTo(bytes, offset + 2);
            offset += 2 + widths[index] * height;
        }
        return bytes;
    }
}
