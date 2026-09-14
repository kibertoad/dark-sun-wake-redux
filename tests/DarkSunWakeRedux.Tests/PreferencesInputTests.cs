using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PreferencesInputTests
{
    [Fact]
    public void ResolvesEveryMappedButtonToAbsoluteCanvasGeometry()
    {
        var controls = PreferencesInput.Resolve(Catalog());

        Assert.Equal(13, controls.Count);
        Assert.Equal(Enum.GetValues<PreferencesAction>(), controls.Select(item => item.Action));
        Assert.Equal((104, 66, 16, 16),
            (controls[0].X, controls[0].Y, controls[0].Width, controls[0].Height));
        Assert.Equal((140, 120, 16, 16),
            (controls[^1].X, controls[^1].Y, controls[^1].Width, controls[^1].Height));
    }

    [Fact]
    public void HitTestingUsesExclusiveRectangleEdges()
    {
        var controls = PreferencesInput.Resolve(Catalog());
        var first = controls[0];

        Assert.Equal(first, PreferencesInput.HitTest(controls, first.X, first.Y));
        Assert.Equal(first, PreferencesInput.HitTest(
            controls, first.X + first.Width - 1, first.Y + first.Height - 1));
        Assert.Null(PreferencesInput.HitTest(controls, first.X + first.Width, first.Y));
        Assert.Null(PreferencesInput.HitTest(controls, first.X, first.Y + first.Height));
    }

    [Fact]
    public void RoutesOnlyEvidencedNavigationControls()
    {
        var controls = PreferencesInput.Resolve(Catalog());
        var gameMenu = Assert.Single(controls,
            item => item.Action == PreferencesAction.GameMenu);
        var returnToGame = Assert.Single(controls,
            item => item.Action == PreferencesAction.ReturnToGame);

        Assert.Equal(ExplorationView.GameMenu,
            PreferencesInput.CommandFor(gameMenu)!.View);
        Assert.Equal(ExplorationCommandKind.Escape,
            PreferencesInput.CommandFor(returnToGame)!.Kind);
        Assert.All(controls.Except([gameMenu, returnToGame]),
            item => Assert.Null(PreferencesInput.CommandFor(item)));
    }

    [Fact]
    public void RejectsAControlWhoseMappedImageDoesNotMatch()
    {
        var catalog = Catalog();
        var buttons = catalog.Buttons.ToArray();
        buttons[0] = buttons[0] with { ImageResourceNumber = 999 };

        Assert.Throws<InvalidDataException>(() => PreferencesInput.Resolve(
            catalog with { Buttons = buttons }));
    }

    [Fact]
    public void RejectsADuplicatedMappedButtonReference()
    {
        var catalog = Catalog();
        var children = catalog.Windows[0].Children.ToArray();
        children[^1] = children[0];
        var window = catalog.Windows[0] with { Children = children };

        var error = Assert.Throws<InvalidDataException>(() => PreferencesInput.Resolve(
            catalog with { Windows = [window] }));

        Assert.Contains("exactly one BUTN #16300", error.Message);
    }

    private static PackedUiCatalog Catalog()
    {
        var children = OriginalContent.PreferencesButtons.Select(asset =>
            new UiChildReference("BUTN", asset.ButtonResourceNumber,
                checked((short)asset.X), checked((short)asset.Y))).ToArray();
        var window = new UiWindowResource(OriginalContent.PreferencesWindowResourceNumber, 0,
            checked((ushort)OriginalContent.GameMenuLayer.FrameWidth),
            checked((ushort)OriginalContent.GameMenuLayer.FrameHeight), children);
        var buttons = OriginalContent.PreferencesButtons.Select(asset =>
            new UiButtonResource(asset.ButtonResourceNumber,
                checked((ushort)asset.FrameWidth), checked((ushort)asset.FrameHeight),
                asset.ImageResourceNumber, 0)).ToArray();
        return new([window], buttons, [], []);
    }
}
