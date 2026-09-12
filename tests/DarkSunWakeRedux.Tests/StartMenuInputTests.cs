using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartMenuInputTests
{
    [Theory]
    [InlineData(19300U, StartWindowChoice.StartGame)]
    [InlineData(19301U, StartWindowChoice.CreateCharacters)]
    [InlineData(19302U, StartWindowChoice.LoadSavedGame)]
    [InlineData(19303U, StartWindowChoice.ExitToDos)]
    public void DsuiControlRectanglesMapResourceIdentityToSemanticChoices(
        uint resourceNumber,
        StartWindowChoice expected)
    {
        var controls = StartMenuInput.Resolve(Catalog());
        var control = Assert.Single(controls, item => item.ButtonResourceNumber == resourceNumber);

        Assert.Equal(expected, StartMenuInput.HitTest(controls, control.X, control.Y));
        Assert.Equal(expected, StartMenuInput.HitTest(
            controls, control.X + control.Width - 1, control.Y + control.Height - 1));
        Assert.NotEqual(expected, StartMenuInput.HitTest(controls, control.X + control.Width, control.Y));
    }

    [Fact]
    public void ResolutionUsesCatalogOrderCoordinatesDimensionsAndImageReferences()
    {
        var controls = StartMenuInput.Resolve(Catalog());

        Assert.Equal([19303U, 19301U, 19300U, 19302U],
            controls.Select(item => item.ButtonResourceNumber));
        var create = Assert.Single(controls, item => item.Choice == StartWindowChoice.CreateCharacters);
        Assert.Equal((50, 87, 220, 12, 19112U),
            (create.X, create.Y, create.Width, create.Height, create.ImageResourceNumber));
    }

    [Fact]
    public void RejectsIncompleteUnexpectedOrMismatchedStartGraph()
    {
        var catalog = Catalog();
        var incomplete = catalog with
        {
            Windows = [catalog.Windows[0] with { Children = catalog.Windows[0].Children.Skip(1).ToArray() }]
        };
        Assert.Throws<InvalidDataException>(() => StartMenuInput.Resolve(incomplete));

        var extraButton = new UiButtonResource(999, 1, 1, 0, 0);
        var unexpected = catalog with
        {
            Windows = [catalog.Windows[0] with
            {
                Children = [.. catalog.Windows[0].Children, new UiChildReference("BUTN", 999, 0, 0)]
            }],
            Buttons = [.. catalog.Buttons, extraButton]
        };
        Assert.Throws<InvalidDataException>(() => StartMenuInput.Resolve(unexpected));

        var mismatched = catalog with
        {
            Buttons = catalog.Buttons.Select(button => button.ResourceNumber == 19300
                ? button with { ImageResourceNumber = 999 }
                : button).ToArray()
        };
        Assert.Throws<InvalidDataException>(() => StartMenuInput.Resolve(mismatched));
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
        var controls = StartMenuInput.Resolve(Catalog());
        var button = Assert.Single(controls, item => item.Choice == StartWindowChoice.CreateCharacters);
        Assert.True(transform.TryToLogical(
            transform.X + button.X * 3, transform.Y + button.Y * 3, out var x, out var y));
        var session = new StartFlowSession(0);

        var result = session.Execute(StartFlowCommand.Choose(
            StartMenuInput.HitTest(controls, x, y)!.Value));

        Assert.True(result.Accepted);
        Assert.Equal(StartFlowScreen.PartyOverview, session.Snapshot().Screen);
    }

    private static PackedUiCatalog Catalog()
    {
        var children = new[]
        {
            new UiChildReference("BUTN", 19303, 92, 120),
            new UiChildReference("BUTN", 19301, 50, 87),
            new UiChildReference("BUTN", 19300, 94, 70),
            new UiChildReference("BUTN", 19302, 64, 104)
        };
        var geometry = new Dictionary<uint, (ushort Width, ushort Height)>
        {
            [19300] = (127, 12),
            [19301] = (220, 12),
            [19302] = (192, 12),
            [19303] = (127, 12)
        };
        var buttons = OriginalContent.StartMenuButtons.Select(asset =>
            new UiButtonResource(asset.ButtonResourceNumber,
                geometry[asset.ButtonResourceNumber].Width,
                geometry[asset.ButtonResourceNumber].Height,
                asset.ImageResourceNumber, 0)).ToArray();
        return new([new(StartMenuInput.WindowResourceNumber, 19004, 320, 200, children)],
            buttons, [], []);
    }
}
