using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class RegionSceneRasterizerTests
{
    [Fact]
    public void CropsTerrainAcrossTileBoundaries()
    {
        var map = Enumerable.Repeat((byte)1, GffRegion.MapByteCount).ToArray();
        map[1] = 2;
        var region = Region(map: map);

        var viewport = RegionSceneRasterizer.Rasterize(
            region, Objects(), 15, 0, 2, 1);

        Assert.Equal((15, 0, 2, 1),
            (viewport.OriginX, viewport.OriginY, viewport.Width, viewport.Height));
        Assert.Equal([1, 2], viewport.Pixels);
        Assert.Equal([255, 255], viewport.Alpha);
    }

    [Fact]
    public void PlacesMirrorsAndOrdersFirstObjectFrames()
    {
        var entities = new[]
        {
            new RegionEntityReference(5, 5, 1, RegionSceneRasterizer.MirroredEntityFlag, 10),
            new RegionEntityReference(5, 4, 0, 0, 11)
        };
        var objects = new PackedObjectFrameCatalog(
        [
            new PackedObjectFrameDefinition(10, 0, 1, 2, 0, 0, 0, 5),
            new PackedObjectFrameDefinition(11, 0, 0, 2, 0, 0, 0, 6)
        ],
        [
            new PackedObjectImage(5, [Frame(2, 1, [3, 4])]),
            new PackedObjectImage(6, [Frame(1, 1, [9])])
        ]);

        var viewport = RegionSceneRasterizer.Rasterize(
            Region(entities: entities), objects, 3, 2, 4, 1);

        Assert.Equal([1, 4, 9, 1], viewport.Pixels);
        Assert.All(viewport.Alpha, value => Assert.Equal(255, value));
    }

    [Fact]
    public void ClipsObjectsAndLeavesTransparentPixelsUnchanged()
    {
        var region = Region(entities:
        [
            new RegionEntityReference(0, 0, 0, 0, 10),
            new RegionEntityReference(0, 0, 0, 0, 11)
        ]);
        var objects = new PackedObjectFrameCatalog(
        [
            new PackedObjectFrameDefinition(10, 0, 1, 0, 0, 0, 0, 5),
            new PackedObjectFrameDefinition(11, 0, 0, 0, 0, 0, 0, 6)
        ],
        [
            new PackedObjectImage(5, [Frame(2, 1, [7, 8])]),
            new PackedObjectImage(6, [Frame(1, 1, [9], [0])])
        ]);

        var viewport = RegionSceneRasterizer.Rasterize(region, objects, 0, 0, 2, 1);

        Assert.Equal([8, 1], viewport.Pixels);
        Assert.Equal([255, 255], viewport.Alpha);
    }

    [Fact]
    public void RejectsInvalidViewportAndDisconnectedContent()
    {
        Assert.Contains("invalid dimensions", Error(Region(), Objects(), 0, 0, 0, 1));
        Assert.Contains("exceeds the 2048x1568 region",
            Error(Region(), Objects(), 2040, 0, 16, 16));

        var missingObject = Region(entities: [new RegionEntityReference(1, 1, 0, 0, 99)]);
        Assert.Contains("missing object definition #99",
            Error(missingObject, Objects(), 0, 0, 1, 1));

        var badMap = Region(map: new byte[GffRegion.MapByteCount - 1]);
        Assert.Contains("12544-byte terrain map", Error(badMap, Objects(), 0, 0, 1, 1));
    }

    private static string Error(
        PackedRegion region,
        PackedObjectFrameCatalog objects,
        int x,
        int y,
        int width,
        int height) => Assert.Throws<InvalidDataException>(() =>
            RegionSceneRasterizer.Rasterize(region, objects, x, y, width, height)).Message;

    private static PackedRegion Region(
        byte[]? map = null,
        IReadOnlyList<RegionEntityReference>? entities = null) => new(
        50,
        "Tyr",
        Enumerable.Repeat(new Rgb24(0, 0, 0), IndexedPalette.ColorCount).ToArray(),
        map ?? Enumerable.Repeat((byte)1, GffRegion.MapByteCount).ToArray(),
        new byte[GffRegion.MapByteCount],
        new Dictionary<byte, IndexedImageFrame>
        {
            [1] = Frame(16, 16, Enumerable.Repeat((byte)1, 256).ToArray()),
            [2] = Frame(16, 16, Enumerable.Repeat((byte)2, 256).ToArray())
        },
        entities ?? []);

    private static PackedObjectFrameCatalog Objects() => new(
        [new PackedObjectFrameDefinition(10, 0, 0, 0, 0, 0, 0, 5)],
        [new PackedObjectImage(5, [Frame(1, 1, [3])])]);

    private static IndexedImageFrame Frame(
        int width,
        int height,
        byte[] pixels,
        byte[]? alpha = null) => new(
        width, height, pixels, alpha ?? Enumerable.Repeat((byte)255, pixels.Length).ToArray());
}
