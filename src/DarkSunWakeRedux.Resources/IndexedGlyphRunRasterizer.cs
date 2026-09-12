namespace DarkSunWakeRedux.Resources;

public sealed record IndexedGlyphRun(int Width, int Height, byte[] Pixels);

public static class IndexedGlyphRunRasterizer
{
    public const int MaximumWidth = 4096;
    public const int MaximumPixels = MaximumWidth * IndexedBitmapFont.MaximumGlyphHeight;

    public static IndexedGlyphRun Rasterize(
        PackedIndexedBitmapFont font,
        ReadOnlySpan<byte> glyphIndices,
        int interGlyphSpacing = 0)
    {
        ArgumentNullException.ThrowIfNull(font);
        if (interGlyphSpacing < 0)
            throw new ArgumentOutOfRangeException(nameof(interGlyphSpacing), "Glyph spacing cannot be negative.");
        if (font.Glyphs.Count != IndexedBitmapFont.CharacterCount)
            throw new InvalidDataException($"Font must contain {IndexedBitmapFont.CharacterCount} glyphs.");

        var height = ValidateGlyphs(font.Glyphs);
        long measuredWidth = 0;
        for (var index = 0; index < glyphIndices.Length; index++)
        {
            measuredWidth += font.Glyphs[glyphIndices[index]].Width;
            if (index + 1 < glyphIndices.Length) measuredWidth += interGlyphSpacing;
            if (measuredWidth > MaximumWidth)
                throw new InvalidDataException($"Rasterized glyph run width exceeds the {MaximumWidth}-pixel safety limit.");
        }

        var width = (int)measuredWidth;
        var pixelCount = checked(width * height);
        if (pixelCount > MaximumPixels)
            throw new InvalidDataException($"Rasterized glyph run exceeds the {MaximumPixels}-pixel safety limit.");
        var pixels = new byte[pixelCount];
        var destinationX = 0;
        for (var runIndex = 0; runIndex < glyphIndices.Length; runIndex++)
        {
            var glyph = font.Glyphs[glyphIndices[runIndex]];
            for (var row = 0; row < height; row++)
            {
                glyph.Pixels.AsSpan(row * glyph.Width, glyph.Width)
                    .CopyTo(pixels.AsSpan(row * width + destinationX, glyph.Width));
            }
            destinationX += glyph.Width;
            if (runIndex + 1 < glyphIndices.Length) destinationX += interGlyphSpacing;
        }
        return new(width, height, pixels);
    }

    private static int ValidateGlyphs(IReadOnlyList<IndexedFontGlyph> glyphs)
    {
        var height = glyphs[0].Height;
        if (height is <= 0 or > IndexedBitmapFont.MaximumGlyphHeight)
            throw new InvalidDataException($"Font contains invalid glyph height {height}.");
        for (var index = 0; index < glyphs.Count; index++)
        {
            var glyph = glyphs[index];
            if (glyph.Height != height)
                throw new InvalidDataException($"Font glyph {index} height differs from the shared height.");
            if (glyph.Width is < 0 or > IndexedBitmapFont.MaximumGlyphWidth)
                throw new InvalidDataException($"Font glyph {index} width {glyph.Width} exceeds the safety limit.");
            if (glyph.Pixels is null || glyph.Pixels.Length != glyph.Width * glyph.Height)
                throw new InvalidDataException($"Font glyph {index} pixel length does not match its dimensions.");
        }
        return height;
    }
}
