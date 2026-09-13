using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class RegionTerrainGridTests
{
    [Theory]
    [InlineData(0x00, true)]
    [InlineData(0x20, true)]
    [InlineData(0x40, false)]
    [InlineData(0x80, true)]
    [InlineData(0xc0, false)]
    [InlineData(0xff, false)]
    public void UsesOnlyTheEvidencedBlockingBit(byte flags, bool expected)
    {
        var navigation = Navigation(flags);

        Assert.Equal(expected, navigation.IsTerrainOpen(2, 3));
        Assert.Equal(flags, navigation.FlagsAt(2, 3));
    }

    [Fact]
    public void TreatsOutOfBoundsAsImpassableAndRejectsDirectReads()
    {
        var navigation = Navigation(0);

        Assert.False(navigation.IsTerrainOpen(-1, 0));
        Assert.False(navigation.IsTerrainOpen(0, -1));
        Assert.False(navigation.IsTerrainOpen(navigation.Width, 0));
        Assert.False(navigation.IsTerrainOpen(0, navigation.Height));
        Assert.Throws<ArgumentOutOfRangeException>(() => navigation.FlagsAt(-1, 0));
    }

    [Fact]
    public void ConvertsWorldPixelsAndCellCentersAtExactTileEdges()
    {
        var navigation = Navigation(0);

        Assert.Equal((0, 0), navigation.CellAtWorldPixel(0, 0));
        Assert.Equal((1, 1), navigation.CellAtWorldPixel(16, 31));
        Assert.Equal((24, 24), navigation.CellCenter(1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            navigation.CellAtWorldPixel(navigation.Width * 16, 0));
    }

    [Fact]
    public void CountsPassableCellsWithoutTreatingUnknownHighBitAsBlocking()
    {
        var geometry = Enumerable.Repeat((byte)0x40, GffRegion.MapByteCount).ToArray();
        geometry[0] = 0;
        geometry[1] = 0x80;

        Assert.Equal(2, new RegionTerrainGrid(Region(geometry)).OpenCellCount);
    }

    [Fact]
    public void OwnsAnImmutableCopyOfTheSourcePlane()
    {
        var geometry = new byte[GffRegion.MapByteCount];
        var terrain = new RegionTerrainGrid(Region(geometry));

        geometry[0] = RegionTerrainGrid.BlockingMask;

        Assert.True(terrain.IsTerrainOpen(0, 0));
        Assert.Equal(0, terrain.FlagsAt(0, 0));
    }

    [Fact]
    public void RejectsMalformedGeometryPlane()
    {
        var region = Region([]);

        Assert.Throws<InvalidDataException>(() => new RegionTerrainGrid(region));
    }

    private static RegionTerrainGrid Navigation(byte value)
    {
        var geometry = new byte[GffRegion.MapByteCount];
        geometry[3 * GffRegion.TileColumns + 2] = value;
        return new(Region(geometry));
    }

    private static PackedRegion Region(byte[] geometry) => new(
        50, "Synthetic", Array.Empty<Rgb24>(), new byte[GffRegion.MapByteCount],
        geometry, new Dictionary<byte, IndexedImageFrame>(),
        Array.Empty<RegionEntityReference>());
}
