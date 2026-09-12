namespace DarkSunWakeRedux.Resources;

public sealed record IndexedGlyphBlock(int Width, int Height, byte[] Pixels);

public static class IndexedGlyphBlockRasterizer
{
    public const int MaximumHeight = 4096;
    public const int MaximumPixels = IndexedGlyphRunRasterizer.MaximumWidth * MaximumHeight;

    public static IndexedGlyphBlock Rasterize(
        PackedIndexedBitmapFont font,
        IReadOnlyList<ReadOnlyMemory<byte>> lines,
        int interGlyphSpacing = 0,
        int interLineSpacing = 0)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(lines);
        if (interLineSpacing < 0)
            throw new ArgumentOutOfRangeException(nameof(interLineSpacing), "Line spacing cannot be negative.");

        var emptyRun = IndexedGlyphRunRasterizer.Rasterize(font, [], interGlyphSpacing);
        if (lines.Count == 0) return new(0, 0, []);

        var measuredHeight = checked((long)lines.Count * emptyRun.Height +
            (long)(lines.Count - 1) * interLineSpacing);
        if (measuredHeight > MaximumHeight)
            throw new InvalidDataException(
                $"Rasterized glyph block height exceeds the {MaximumHeight}-pixel safety limit.");

        var runs = new IndexedGlyphRun[lines.Count];
        var width = 0;
        for (var index = 0; index < lines.Count; index++)
        {
            runs[index] = IndexedGlyphRunRasterizer.Rasterize(
                font, lines[index].Span, interGlyphSpacing);
            width = Math.Max(width, runs[index].Width);
        }

        var height = (int)measuredHeight;
        var pixelCount = checked((long)width * height);
        if (pixelCount > MaximumPixels)
            throw new InvalidDataException(
                $"Rasterized glyph block exceeds the {MaximumPixels}-pixel safety limit.");
        var pixels = new byte[(int)pixelCount];
        var destinationY = 0;
        foreach (var run in runs)
        {
            for (var row = 0; row < run.Height; row++)
                run.Pixels.AsSpan(row * run.Width, run.Width)
                    .CopyTo(pixels.AsSpan((destinationY + row) * width, run.Width));
            destinationY += run.Height + interLineSpacing;
        }
        return new(width, height, pixels);
    }
}
