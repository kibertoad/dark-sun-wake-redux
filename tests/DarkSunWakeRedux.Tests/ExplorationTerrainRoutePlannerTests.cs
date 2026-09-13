using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationTerrainRoutePlannerTests
{
    [Fact]
    public void MapsLogicalClickThroughCameraToAStableTerrainRoute()
    {
        var navigation = Navigation((1, 1));
        var snapshot = Snapshot(cameraX: 16, cameraY: 32);

        var first = ExplorationTerrainRoutePlanner.PlanAt(
            navigation, snapshot, new(1, 2), logicalX: 36, logicalY: 20);
        var second = ExplorationTerrainRoutePlanner.PlanAt(
            navigation, snapshot, new(1, 2), logicalX: 36, logicalY: 20);

        Assert.NotNull(first);
        Assert.True(first.Found);
        Assert.Equal(first.Points, second!.Points);
        Assert.Equal(new GridPoint(3, 3), first.Points[^1]);
        Assert.DoesNotContain(new GridPoint(1, 1), first.Points);
    }

    [Fact]
    public void ReportsBlockedDestinationAsUnreachable()
    {
        var navigation = Navigation((2, 2));

        var route = ExplorationTerrainRoutePlanner.PlanAt(
            navigation, Snapshot(), new(0, 0), logicalX: 32, logicalY: 32);

        Assert.NotNull(route);
        Assert.False(route.Found);
        Assert.Empty(route.Points);
    }

    [Theory]
    [InlineData(ExplorationCursorMode.Attack, ExplorationView.World, 10, 10)]
    [InlineData(ExplorationCursorMode.Look, ExplorationView.World, 10, 10)]
    [InlineData(ExplorationCursorMode.Walk, ExplorationView.GameMenu, 10, 10)]
    [InlineData(ExplorationCursorMode.Walk, ExplorationView.World, -1, 10)]
    [InlineData(ExplorationCursorMode.Walk, ExplorationView.World, 320, 10)]
    public void IgnoresClicksOutsideActiveWalkMode(
        ExplorationCursorMode mode, ExplorationView view, int x, int y)
    {
        Assert.Null(ExplorationTerrainRoutePlanner.PlanAt(
            Navigation(), Snapshot(mode: mode, view: view), new(0, 0), x, y));
    }

    private static ExplorationSnapshot Snapshot(
        int cameraX = 0,
        int cameraY = 0,
        ExplorationCursorMode mode = ExplorationCursorMode.Walk,
        ExplorationView view = ExplorationView.World) =>
        new(cameraX, cameraY, mode, PartyDisplayMode.LeaderOnly, view);

    private static RegionTerrainGrid Navigation(params (int X, int Y)[] blocked)
    {
        var geometry = new byte[GffRegion.MapByteCount];
        foreach (var point in blocked)
            geometry[point.Y * GffRegion.TileColumns + point.X] = RegionTerrainGrid.BlockingMask;
        var region = new PackedRegion(50, "Synthetic", Array.Empty<Rgb24>(),
            new byte[GffRegion.MapByteCount], geometry,
            new Dictionary<byte, IndexedImageFrame>(), Array.Empty<RegionEntityReference>());
        return new(region);
    }
}
