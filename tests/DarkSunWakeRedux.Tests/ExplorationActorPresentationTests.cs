using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationActorPresentationTests
{
    [Fact]
    public void MapsWorldSpriteBoundsThroughAnyCamera()
    {
        var actor = new ExplorationActorPresentation(17, 35, 0, 3);

        Assert.Equal(new LogicalSpriteBounds(160, 91, 17, 35),
            actor.AtAnchor(new(74, 91), OpeningTyrScene.OriginX,
                OpeningTyrScene.OriginY, GffRegion.TilePixelSize));
        Assert.Equal(new LogicalSpriteBounds(-16, -41, 17, 35),
            actor.AtAnchor(new(74, 91), 1_200, 1_500, GffRegion.TilePixelSize));
    }

    [Theory]
    [InlineData(-16, 20, true)]
    [InlineData(-17, 20, false)]
    [InlineData(319, 199, true)]
    [InlineData(320, 199, false)]
    [InlineData(319, 200, false)]
    public void UsesRectangleEdgesForViewportVisibility(int x, int y, bool expected)
    {
        Assert.Equal(expected, new LogicalSpriteBounds(x, y, 17, 35).Intersects(320, 200));
    }

    [Fact]
    public void UsesWidenedRectangleEdgesForExtremeViewportCoordinates()
    {
        var bounds = new LogicalSpriteBounds(int.MaxValue - 1, int.MaxValue - 1, 2, 2);

        Assert.True(bounds.Intersects(int.MaxValue, int.MaxValue));
    }

    [Fact]
    public void OpeningAnchorUsesTheObservedActorCoordinateFields()
    {
        Assert.Equal((74, 91),
            (OpeningTyrScene.LeaderAnchorCellX, OpeningTyrScene.LeaderAnchorCellY));
    }

    [Fact]
    public void ResolvesWorldCenterFromTheSameAnchorGeometry()
    {
        var actor = new ExplorationActorPresentation(17, 35, 0, 3);

        Assert.Equal((1_192, 1_476),
            actor.WorldCenterAtAnchor(new(74, 91), GffRegion.TilePixelSize));
    }

    [Fact]
    public void ResolvesWorldCenterFromInterpolatedVisualPosition()
    {
        var actor = new ExplorationActorPresentation(17, 35, 0, 3);
        var movement = new ExplorationActorVisualSnapshot(
            new(74, 91), new(75, 91), 1, 2);

        Assert.Equal((1_200, 1_476),
            actor.WorldCenterAtMovement(movement, GffRegion.TilePixelSize));
    }

    [Theory]
    [InlineData(1, 1, 2, 1, 4, 8, 24, 16)]
    [InlineData(2, 2, 1, 1, 1, 4, 28, 28)]
    [InlineData(1, 1, 2, 2, 8, 8, 32, 32)]
    public void InterpolatesActorBoundsWithoutFloatingPoint(
        int fromX,
        int fromY,
        int toX,
        int toY,
        long progress,
        long duration,
        int expectedX,
        int expectedY)
    {
        var actor = new ExplorationActorPresentation(17, 35, 0, 0);
        var movement = new ExplorationActorVisualSnapshot(
            new(fromX, fromY), new(toX, toY), progress, duration);

        var bounds = actor.AtMovement(movement, 0, 0, 16);

        Assert.Equal((expectedX, expectedY), (bounds.X, bounds.Y));
    }

    [Theory]
    [InlineData(-1, 8)]
    [InlineData(9, 8)]
    [InlineData(0, 0)]
    public void RejectsInvalidInterpolationProgress(long progress, long duration)
    {
        var actor = new ExplorationActorPresentation(17, 35, 0, 0);
        var movement = new ExplorationActorVisualSnapshot(
            new(1, 1), new(2, 1), progress, duration);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            actor.AtMovement(movement, 0, 0, 16));
    }
}
