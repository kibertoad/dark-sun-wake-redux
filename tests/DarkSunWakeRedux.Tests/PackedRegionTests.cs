using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedRegionTests
{
    [Fact]
    public void RoundTripsEveryBoundedRegionField()
    {
        var expected = Region(reverseTiles: true);

        using var stream = new MemoryStream();
        expected.Write(stream);
        stream.Position = 0;
        var actual = PackedRegion.Read(stream, "synthetic.dsrg");

        Assert.Equal(expected.ResourceNumber, actual.ResourceNumber);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Palette, actual.Palette);
        Assert.Equal(expected.TileMap, actual.TileMap);
        Assert.Equal(expected.GeometryMap, actual.GeometryMap);
        Assert.Equal([2, 7], actual.Tiles.Keys);
        Assert.Equal(expected.Tiles[7].Pixels, actual.Tiles[7].Pixels);
        Assert.Equal(expected.Tiles[7].Alpha, actual.Tiles[7].Alpha);
        Assert.Equal(expected.Entities, actual.Entities);
    }

    [Fact]
    public void WritesTilesInCanonicalOrder()
    {
        Assert.Equal(Write(Region(reverseTiles: false)), Write(Region(reverseTiles: true)));
    }

    [Fact]
    public void RejectsInvalidHeaderLengthAndTrailingData()
    {
        var signature = Write(Region());
        signature[0] = (byte)'X';
        Assert.Contains("signature", ReadError(signature));

        var dimensions = Write(Region());
        dimensions[10] = 1;
        Assert.Contains("dimensions", ReadError(dimensions));

        var trailing = Write(Region()).Append((byte)0).ToArray();
        Assert.Contains("content length", ReadError(trailing));
    }

    [Fact]
    public void RejectsMissingNoncanonicalAndNonBinaryTileData()
    {
        const int headerBytes = 24;
        const int nameBytes = 3;
        const int paletteBytes = IndexedPalette.ColorCount * 3;
        const int tilePixels = GffRegion.TilePixelSize * GffRegion.TilePixelSize;
        var mapStart = headerBytes + nameBytes + paletteBytes;
        var tilesStart = mapStart + GffRegion.MapByteCount * 2;

        var missing = Write(Region());
        missing[mapStart] = 9;
        Assert.Contains("missing TILE #9", ReadError(missing));

        var duplicate = Write(Region());
        duplicate[tilesStart + 1 + tilePixels * 2] = 2;
        Assert.Contains("canonical order", ReadError(duplicate));

        var alpha = Write(Region());
        alpha[tilesStart + 1 + tilePixels] = 1;
        Assert.Contains("non-binary alpha", ReadError(alpha));
    }

    [Fact]
    public void RejectsInvalidInMemoryRegionBeforeWriting()
    {
        var region = Region() with
        {
            TileMap = Enumerable.Repeat((byte)9, GffRegion.MapByteCount).ToArray()
        };

        using var stream = new MemoryStream();
        Assert.Contains("missing TILE #9",
            Assert.Throws<InvalidDataException>(() => region.Write(stream)).Message);
        Assert.Empty(stream.ToArray());
    }

    private static PackedRegion Region(bool reverseTiles = false)
    {
        var tileMap = Enumerable.Repeat((byte)2, GffRegion.MapByteCount).ToArray();
        tileMap[^1] = 7;
        var geometry = new byte[GffRegion.MapByteCount];
        geometry[17] = 0xe0;
        var first = Tile(3, 255);
        var second = Tile(4, 0);
        IReadOnlyDictionary<byte, IndexedImageFrame> tiles = reverseTiles
            ? new Dictionary<byte, IndexedImageFrame> { [7] = second, [2] = first }
            : new Dictionary<byte, IndexedImageFrame> { [2] = first, [7] = second };
        return new(50, "Tyr",
            Enumerable.Range(0, IndexedPalette.ColorCount)
                .Select(value => new Rgb24((byte)value, (byte)(255 - value), (byte)(value / 2)))
                .ToArray(),
            tileMap, geometry, tiles,
            [new RegionEntityReference(24, 40, -3, 0x80, 10),
                new RegionEntityReference(72, 88, 4, 0x21, -11)]);
    }

    private static IndexedImageFrame Tile(byte pixel, byte alpha) => new(
        GffRegion.TilePixelSize,
        GffRegion.TilePixelSize,
        Enumerable.Repeat(pixel, GffRegion.TilePixelSize * GffRegion.TilePixelSize).ToArray(),
        Enumerable.Repeat(alpha, GffRegion.TilePixelSize * GffRegion.TilePixelSize).ToArray());

    private static byte[] Write(PackedRegion region)
    {
        using var stream = new MemoryStream();
        region.Write(stream);
        return stream.ToArray();
    }

    private static string ReadError(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return Assert.Throws<InvalidDataException>(() => PackedRegion.Read(stream, "bad.dsrg")).Message;
    }
}
