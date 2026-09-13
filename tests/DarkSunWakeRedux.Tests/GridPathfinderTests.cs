using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GridPathfinderTests
{
    [Fact]
    public void UsesOptimalDiagonalRouteOnOpenGrid()
    {
        var result = GridPathfinder.FindPath(5, 5, new(0, 0), new(3, 2), _ => true);

        Assert.True(result.Found);
        Assert.Equal([new(0, 0), new(1, 1), new(2, 2), new(3, 2)], result.Points);
    }

    [Fact]
    public void FindsStableDetourAroundBarrier()
    {
        var blocked = new HashSet<GridPoint> { new(1, 0), new(1, 1), new(1, 2) };

        var first = GridPathfinder.FindPath(4, 4, new(0, 1), new(3, 1),
            point => !blocked.Contains(point));
        var second = GridPathfinder.FindPath(4, 4, new(0, 1), new(3, 1),
            point => !blocked.Contains(point));

        Assert.True(first.Found);
        Assert.Equal(first.Points, second.Points);
        Assert.DoesNotContain(first.Points, blocked.Contains);
    }

    [Fact]
    public void DoesNotCutDiagonalCorner()
    {
        var blocked = new HashSet<GridPoint> { new(1, 0), new(0, 1) };

        var result = GridPathfinder.FindPath(2, 2, new(0, 0), new(1, 1),
            point => !blocked.Contains(point));

        Assert.False(result.Found);
        Assert.Empty(result.Points);
    }

    [Fact]
    public void RejectsBlockedEndpointsAndReturnsStartForZeroLengthRoute()
    {
        Assert.False(GridPathfinder.FindPath(2, 2, new(0, 0), new(1, 1),
            point => point != new GridPoint(1, 1)).Found);

        var same = GridPathfinder.FindPath(2, 2, new(1, 1), new(1, 1), _ => true);
        Assert.Equal([new GridPoint(1, 1)], same.Points);
    }

    [Fact]
    public void RejectsUnsafeGridAndEndpointBounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GridPathfinder.FindPath(0, 1, new(0, 0), new(0, 0), _ => true));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GridPathfinder.FindPath(GridPathfinder.MaximumCellCount, 2,
                new(0, 0), new(0, 0), _ => true));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GridPathfinder.FindPath(2, 2, new(-1, 0), new(0, 0), _ => true));
    }
}
