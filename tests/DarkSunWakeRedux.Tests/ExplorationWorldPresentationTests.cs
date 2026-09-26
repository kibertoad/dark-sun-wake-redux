using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationWorldPresentationTests
{
    [Fact]
    public void ExpandsOnlyAnUnobscuredWorldView()
    {
        Assert.True(ExplorationWorldPresentation.UsesExpandedWorld(
            ExplorationView.World, dialogueVisible: false, wideMapView: true));
        Assert.False(ExplorationWorldPresentation.UsesExpandedWorld(
            ExplorationView.World, dialogueVisible: true, wideMapView: true));
    }

    [Theory]
    [InlineData(ExplorationView.GameMenu)]
    [InlineData(ExplorationView.Preferences)]
    [InlineData(ExplorationView.ViewInventory)]
    public void DoesNotExpandAFixedCanvasDestination(ExplorationView view) =>
        Assert.False(ExplorationWorldPresentation.UsesExpandedWorld(
            view, dialogueVisible: false, wideMapView: true));

    // DEV-EXPLORE-001: with Wide map view off the map keeps the original's framing.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WideMapViewOffNeverExpands(bool dialogueVisible) =>
        Assert.False(ExplorationWorldPresentation.UsesExpandedWorld(
            ExplorationView.World, dialogueVisible, wideMapView: false));
}
