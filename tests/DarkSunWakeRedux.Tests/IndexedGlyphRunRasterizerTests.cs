using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class IndexedGlyphRunRasterizerTests
{
    [Fact]
    public void ComposesVariableWidthGlyphsWithoutChangingPaletteIndices()
    {
        var font = Font();
        var glyphs = font.Glyphs.ToArray();
        glyphs[1] = new IndexedFontGlyph(2, 2, [1, 2, 3, 4]);
        glyphs[2] = new IndexedFontGlyph(1, 2, [5, 6]);
        font = font with { Glyphs = glyphs };

        var run = IndexedGlyphRunRasterizer.Rasterize(font, [1, 2]);

        Assert.Equal(3, run.Width);
        Assert.Equal(2, run.Height);
        Assert.Equal([1, 2, 5, 3, 4, 6], run.Pixels);
    }

    [Fact]
    public void ZeroFillsCallerSelectedSpacingAndSupportsZeroWidthGlyphs()
    {
        var font = Font();
        var glyphs = font.Glyphs.ToArray();
        glyphs[1] = new IndexedFontGlyph(1, 2, [7, 8]);
        font = font with { Glyphs = glyphs };

        var run = IndexedGlyphRunRasterizer.Rasterize(font, [1, 0, 1], interGlyphSpacing: 1);

        Assert.Equal(4, run.Width);
        Assert.Equal([7, 0, 0, 7, 8, 0, 0, 8], run.Pixels);
    }

    [Fact]
    public void EmptyRunRetainsFontHeight()
    {
        var run = IndexedGlyphRunRasterizer.Rasterize(Font(), []);

        Assert.Equal(0, run.Width);
        Assert.Equal(2, run.Height);
        Assert.Empty(run.Pixels);
    }

    [Fact]
    public void RejectsNegativeSpacingAndOversizedOutput()
    {
        var font = Font();
        var glyphs = font.Glyphs.ToArray();
        glyphs[1] = new IndexedFontGlyph(256, 2, new byte[512]);
        font = font with { Glyphs = glyphs };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexedGlyphRunRasterizer.Rasterize(font, [1], -1));
        Assert.Throws<InvalidDataException>(() =>
            IndexedGlyphRunRasterizer.Rasterize(font, Enumerable.Repeat((byte)1, 17).ToArray()));
    }

    [Fact]
    public void RejectsMalformedInMemoryFonts()
    {
        var font = Font();
        var glyphs = font.Glyphs.ToArray();
        glyphs[1] = new IndexedFontGlyph(1, 3, [1, 2, 3]);
        font = font with { Glyphs = glyphs };

        Assert.Throws<InvalidDataException>(() => IndexedGlyphRunRasterizer.Rasterize(font, [1]));
    }

    private static PackedIndexedBitmapFont Font()
    {
        var map = Enumerable.Range(0, IndexedBitmapFont.CharacterCount).Select(value => (byte)value).ToArray();
        var glyphs = Enumerable.Range(0, IndexedBitmapFont.CharacterCount)
            .Select(_ => new IndexedFontGlyph(0, 2, []))
            .ToArray();
        return new(map, glyphs);
    }
}
