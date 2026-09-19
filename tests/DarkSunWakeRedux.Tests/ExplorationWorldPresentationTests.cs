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
            ExplorationView.World, dialogueVisible: false));
        Assert.False(ExplorationWorldPresentation.UsesExpandedWorld(
            ExplorationView.World, dialogueVisible: true));
    }

    [Theory]
    [InlineData(ExplorationView.GameMenu)]
    [InlineData(ExplorationView.Preferences)]
    [InlineData(ExplorationView.ViewInventory)]
    public void DoesNotExpandAFixedCanvasDestination(ExplorationView view) =>
        Assert.False(ExplorationWorldPresentation.UsesExpandedWorld(
            view, dialogueVisible: false));
}
