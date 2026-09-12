using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartMenuInputTests
{
    [Theory]
    [InlineData(0, StartWindowChoice.StartGame)]
    [InlineData(1, StartWindowChoice.CreateCharacters)]
    [InlineData(2, StartWindowChoice.LoadSavedGame)]
    [InlineData(3, StartWindowChoice.ExitToDos)]
    public void DeclaredControlRectanglesMapToSemanticChoices(int index, StartWindowChoice expected)
    {
        var button = OriginalContent.StartMenuButtons[index];

        Assert.Equal(expected, StartMenuInput.HitTest(button.X, button.Y));
        Assert.Equal(expected, StartMenuInput.HitTest(
            button.X + button.ControlWidth - 1, button.Y + button.ControlHeight - 1));
        Assert.Null(StartMenuInput.HitTest(button.X + button.ControlWidth, button.Y));
    }

    [Theory]
    [InlineData(960, 600, 0, 0, 3f)]
    [InlineData(1000, 600, 20, 0, 3f)]
    [InlineData(960, 700, 0, 50, 3f)]
    public void CanvasTransformLetterboxesWithoutChangingLogicalCoordinates(
        int width, int height, int expectedX, int expectedY, float expectedScale)
    {
        var transform = new LogicalCanvasTransform(width, height);

        Assert.Equal(expectedX, transform.X);
        Assert.Equal(expectedY, transform.Y);
        Assert.Equal(expectedScale, transform.Scale);
        Assert.True(transform.TryToLogical(transform.X + 30, transform.Y + 60, out var x, out var y));
        Assert.Equal((10, 20), (x, y));
        Assert.False(transform.TryToLogical(transform.X - 1, transform.Y, out _, out _));
    }

    [Fact]
    public void ViewportClickDrivesDeterministicStartFlow()
    {
        var transform = new LogicalCanvasTransform(960, 600);
        var button = OriginalContent.StartMenuButtons[1];
        Assert.True(transform.TryToLogical(
            transform.X + button.X * 3, transform.Y + button.Y * 3, out var x, out var y));
        var session = new StartFlowSession(0);

        var result = session.Execute(StartFlowCommand.Choose(StartMenuInput.HitTest(x, y)!.Value));

        Assert.True(result.Accepted);
        Assert.Equal(StartFlowScreen.PartyOverview, session.Snapshot().Screen);
    }
}
