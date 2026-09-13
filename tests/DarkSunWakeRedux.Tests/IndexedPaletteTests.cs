using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class IndexedPaletteTests
{
    [Fact]
    public void DecodesSixBitVgaComponents()
    {
        var payload = new byte[IndexedPalette.EncodedLength];
        payload[3] = 63;
        payload[4] = 32;
        payload[5] = 1;

        var palette = IndexedPalette.Read(payload, "synthetic-pal");

        Assert.Equal(new Rgb24(255, 130, 4), palette.Colors[1]);
    }

    [Fact]
    public void RejectsWrongLength()
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            IndexedPalette.Read(new byte[767], "short-pal"));

        Assert.Contains("exactly 768", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsComponentOutsideVgaRange()
    {
        var payload = new byte[IndexedPalette.EncodedLength];
        payload[42] = 64;

        var exception = Assert.Throws<InvalidDataException>(() =>
            IndexedPalette.Read(payload, "bright-pal"));

        Assert.Contains("6-bit VGA", exception.Message, StringComparison.Ordinal);
    }
}
