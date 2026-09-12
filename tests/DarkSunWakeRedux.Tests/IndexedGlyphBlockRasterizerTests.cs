using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class IndexedGlyphBlockRasterizerTests
{
    [Fact]
    public void ComposesVariableWidthLinesWithCallerSelectedSpacing()
    {
        var font = Font();
        var lines = new ReadOnlyMemory<byte>[] { new byte[] { 1, 2 }, new byte[] { 2 } };

        var block = IndexedGlyphBlockRasterizer.Rasterize(
            font, lines, interGlyphSpacing: 1, interLineSpacing: 1);

        Assert.Equal(4, block.Width);
        Assert.Equal(5, block.Height);
        Assert.Equal([
            1, 2, 0, 5,
            3, 4, 0, 6,
            0, 0, 0, 0,
            5, 0, 0, 0,
            6, 0, 0, 0
        ], block.Pixels);
    }

    [Fact]
    public void EmptyBlockHasNoDimensionsOrPixels()
    {
        var block = IndexedGlyphBlockRasterizer.Rasterize(Font(), []);

        Assert.Equal((0, 0), (block.Width, block.Height));
        Assert.Empty(block.Pixels);
    }

    [Fact]
    public void PreservesBlankLinesAtTheFontHeight()
    {
        var block = IndexedGlyphBlockRasterizer.Rasterize(
            Font(), new ReadOnlyMemory<byte>[] { ReadOnlyMemory<byte>.Empty, new byte[] { 2 } });

        Assert.Equal(1, block.Width);
        Assert.Equal(4, block.Height);
        Assert.Equal([0, 0, 5, 6], block.Pixels);
    }

    [Fact]
    public void RejectsNegativeOrOversizedLineSpacing()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexedGlyphBlockRasterizer.Rasterize(Font(), [], interLineSpacing: -1));
        Assert.Throws<InvalidDataException>(() =>
            IndexedGlyphBlockRasterizer.Rasterize(Font(),
                new ReadOnlyMemory<byte>[] { new byte[] { 1 }, new byte[] { 1 } },
                interLineSpacing: IndexedGlyphBlockRasterizer.MaximumHeight));
    }

    private static PackedIndexedBitmapFont Font()
    {
        var map = Enumerable.Range(0, IndexedBitmapFont.CharacterCount).Select(value => (byte)value).ToArray();
        var glyphs = Enumerable.Range(0, IndexedBitmapFont.CharacterCount)
            .Select(_ => new IndexedFontGlyph(0, 2, []))
            .ToArray();
        glyphs[1] = new(2, 2, [1, 2, 3, 4]);
        glyphs[2] = new(1, 2, [5, 6]);
        return new(map, glyphs);
    }
}
