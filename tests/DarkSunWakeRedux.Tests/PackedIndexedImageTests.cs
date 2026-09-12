using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedIndexedImageTests
{
    [Fact]
    public void RoundTripsPalettePixelsAndAlpha()
    {
        var palette = Enumerable.Range(0, 256)
            .Select(index => new Rgb24((byte)index, (byte)(255 - index), (byte)(index / 2)))
            .ToArray();
        var packed = new PackedIndexedImage(palette,
            [new IndexedImageFrame(2, 1, [3, 9], [255, 0])]);
        using var stream = new MemoryStream();

        packed.Write(stream);
        stream.Position = 0;
        var restored = PackedIndexedImage.Read(stream, "synthetic.dsix");

        Assert.Equal(palette, restored.Palette);
        var frame = Assert.Single(restored.Frames);
        Assert.Equal([3, 9], frame.Pixels);
        Assert.Equal([255, 0], frame.Alpha);
    }

    [Fact]
    public void RejectsNonBinaryAlpha()
    {
        var packed = new PackedIndexedImage(new Rgb24[256],
            [new IndexedImageFrame(1, 1, [0], [127])]);

        var exception = Assert.Throws<InvalidDataException>(() => packed.Write(new MemoryStream()));

        Assert.Contains("non-binary alpha", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsTrailingBytes()
    {
        var packed = new PackedIndexedImage(new Rgb24[256],
            [new IndexedImageFrame(1, 1, [0], [0])]);
        using var stream = new MemoryStream();
        packed.Write(stream);
        stream.WriteByte(1);
        stream.Position = 0;

        var exception = Assert.Throws<InvalidDataException>(() =>
            PackedIndexedImage.Read(stream, "trailing.dsix"));

        Assert.Contains("trailing bytes", exception.Message, StringComparison.Ordinal);
    }
}
