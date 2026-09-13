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
    public void OpeningAnchorUsesTheObservedActorCoordinateFields()
    {
        Assert.Equal((74, 91),
            (OpeningTyrScene.LeaderAnchorCellX, OpeningTyrScene.LeaderAnchorCellY));
    }
}
