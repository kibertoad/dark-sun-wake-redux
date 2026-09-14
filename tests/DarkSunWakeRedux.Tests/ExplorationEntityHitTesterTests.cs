using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationEntityHitTesterTests
{
    [Fact]
    public void ReturnsTopmostOpaqueEntityInRasterOrder()
    {
        var region = Region(
            new RegionEntityReference(10, 10, 0, 0, 10),
            new RegionEntityReference(10, 10, 0, 0, 11));
        var tester = new ExplorationEntityHitTester(region, Catalog(
            (10, 5, Frame([1], [255])),
            (11, 6, Frame([2], [255]))));

        var hit = Assert.IsType<ExplorationEntityHit>(tester.HitTest(10, 10));

        Assert.Equal((1, 11U), (hit.EntityIndex, hit.Entity.ObjectResourceNumber));
    }

    [Fact]
    public void TransparentTopEntityFallsThroughAndMirroringUsesDisplayedPixel()
    {
        var region = Region(
            new RegionEntityReference(10, 10, 0, 0, 10),
            new RegionEntityReference(10, 10, 0, 0, 11),
            new RegionEntityReference(20, 10, 0,
                RegionSceneRasterizer.MirroredEntityFlag, 12));
        var tester = new ExplorationEntityHitTester(region, Catalog(
            (10, 5, Frame([1], [255])),
            (11, 6, Frame([2], [0])),
            (12, 7, Frame([3, 4], [255, 0]))));

        Assert.Equal(10U, tester.HitTest(10, 10)!.Entity.ObjectResourceNumber);
        Assert.Null(tester.HitTest(20, 10));
        Assert.Equal(12U, tester.HitTest(21, 10)!.Entity.ObjectResourceNumber);
        Assert.Null(tester.HitTest(40, 40));
    }

    [Fact]
    public void RejectsDisconnectedCatalogs()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ExplorationEntityHitTester(
                Region(new RegionEntityReference(1, 1, 0, 0, 99)),
                Catalog((10, 5, Frame([1], [255])))));

        Assert.Contains("incomplete object-frame reference", exception.Message);
    }

    private static PackedRegion Region(params RegionEntityReference[] entities) => new(
        50, "Tyr", [], [], [], new Dictionary<byte, IndexedImageFrame>(), entities);

    private static PackedObjectFrameCatalog Catalog(
        params (uint Definition, uint Image, IndexedImageFrame Frame)[] entries) => new(
        entries.Select(item => new PackedObjectFrameDefinition(
            item.Definition, 0, 0, 0, 0, 0, 0, item.Image)).ToArray(),
        entries.Select(item => new PackedObjectImage(item.Image, [item.Frame])).ToArray());

    private static IndexedImageFrame Frame(byte[] pixels, byte[] alpha) =>
        new(pixels.Length, 1, pixels, alpha);
}
