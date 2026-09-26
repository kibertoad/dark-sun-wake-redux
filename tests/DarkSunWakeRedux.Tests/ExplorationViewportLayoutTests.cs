using DarkSunWakeRedux.Game;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationViewportLayoutTests
{
    [Fact]
    public void ExpandsWideViewportAroundOriginalCameraCenter()
    {
        var layout = ExplorationViewportLayout.Resolve(
            1920, 1080, 2048, 1568, 1024, 1368, true);

        Assert.Equal((1006, 1368, 356, 200, true),
            (layout.CameraX, layout.CameraY, layout.LogicalWidth,
                layout.LogicalHeight, layout.Expanded));
        Assert.True(layout.TryToLogical(1919, 1079, out var x, out var y));
        Assert.Equal((355, 199), (x, y));
    }

    [Fact]
    public void ExpandsTallViewportWithoutDistortingBaseSlice()
    {
        var layout = ExplorationViewportLayout.Resolve(
            1000, 1000, 2048, 1568, 1024, 1200, true);

        Assert.Equal((1024, 1140, 320, 320),
            (layout.CameraX, layout.CameraY, layout.LogicalWidth, layout.LogicalHeight));
    }

    [Fact]
    public void ClampsExpandedSliceAtWorldBoundary()
    {
        var layout = ExplorationViewportLayout.Resolve(
            1920, 1080, 2048, 1568, 1728, 1368, true);

        Assert.Equal((1692, 1368), (layout.CameraX, layout.CameraY));
    }

    [Fact]
    public void FixedLayoutRetainsOriginalLetterboxedCanvas()
    {
        var layout = ExplorationViewportLayout.Resolve(
            1920, 1080, 2048, 1568, 1024, 1368, false);

        Assert.Equal((1024, 1368, 320, 200, false),
            (layout.CameraX, layout.CameraY, layout.LogicalWidth,
                layout.LogicalHeight, layout.Expanded));
        Assert.False(layout.TryToLogical(0, 0, out _, out _));
    }
    // DEV-EXPLORE-001: the Wide map view setting decides between the two layouts of the same
    // camera, and both keep the original's 320x200 view at the same scale and centre.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WideMapViewSettingChoosesTheLayout(bool wideMapView)
    {
        var layout = ExplorationViewportLayout.Resolve(1920, 1080, 2048, 1568, 1024, 1368,
            ExplorationWorldPresentation.UsesExpandedWorld(
                DarkSunWakeRedux.Core.ExplorationView.World, false, wideMapView));

        Assert.Equal(wideMapView, layout.Expanded);
        Assert.Equal(wideMapView ? (1006, 1368, 356, 200) : (1024, 1368, 320, 200),
            (layout.CameraX, layout.CameraY, layout.LogicalWidth, layout.LogicalHeight));
        // The display centre shows the same map pixel either way.
        Assert.True(layout.TryToLogical(960, 540, out var x, out var y));
        Assert.Equal((1184, 1468), (layout.CameraX + x, layout.CameraY + y));
        // Off letterboxes: the bars at the sides are outside the map view, so a pointer there
        // neither scrolls nor plans a route.
        Assert.Equal(wideMapView, layout.TryToLogical(10, 540, out _, out _));
    }
}
