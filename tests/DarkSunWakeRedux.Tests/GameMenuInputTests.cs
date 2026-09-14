using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GameMenuInputTests
{
    [Fact]
    public void ResolvesEveryMappedButtonToAbsoluteCanvasGeometry()
    {
        var controls = GameMenuInput.Resolve(Catalog());

        Assert.Equal(14, controls.Count);
        Assert.Equal(Enum.GetValues<GameMenuAction>(), controls.Select(item => item.Action));
        Assert.Equal((104, 66, 16, 16),
            (controls[0].X, controls[0].Y, controls[0].Width, controls[0].Height));
        Assert.Equal((194, 120, 28, 16),
            (controls[^1].X, controls[^1].Y, controls[^1].Width, controls[^1].Height));
    }

    [Fact]
    public void HitTestingUsesExclusiveRectangleEdges()
    {
        var controls = GameMenuInput.Resolve(Catalog());
        var first = controls[0];

        Assert.Equal(first, GameMenuInput.HitTest(controls, first.X, first.Y));
        Assert.Equal(first, GameMenuInput.HitTest(
            controls, first.X + first.Width - 1, first.Y + first.Height - 1));
        Assert.Null(GameMenuInput.HitTest(controls, first.X + first.Width, first.Y));
        Assert.Null(GameMenuInput.HitTest(controls, first.X, first.Y + first.Height));
    }

    [Theory]
    [InlineData(GameMenuAction.ViewCharacter, ExplorationView.ViewCharacter)]
    [InlineData(GameMenuAction.ViewInventory, ExplorationView.ViewInventory)]
    [InlineData(GameMenuAction.CastSpellsOrUsePsionics,
        ExplorationView.CastSpellsOrUsePsionics)]
    [InlineData(GameMenuAction.CurrentSpellEffects, ExplorationView.CurrentSpellEffects)]
    [InlineData(GameMenuAction.Preferences, ExplorationView.Preferences)]
    [InlineData(GameMenuAction.OverheadMap, ExplorationView.OverheadMap)]
    public void RoutesImplementedDestinationActions(
        GameMenuAction action, ExplorationView expected)
    {
        var control = Assert.Single(GameMenuInput.Resolve(Catalog()),
            item => item.Action == action);

        var command = Assert.IsType<ExplorationCommand>(GameMenuInput.CommandFor(control));

        Assert.Equal(ExplorationCommandKind.OpenView, command.Kind);
        Assert.Equal(expected, command.View);
    }

    [Theory]
    [InlineData(GameMenuAction.Walk, ExplorationCursorMode.Walk)]
    [InlineData(GameMenuAction.Look, ExplorationCursorMode.Look)]
    [InlineData(GameMenuAction.Attack, ExplorationCursorMode.Attack)]
    public void RoutesExplicitCursorModes(
        GameMenuAction action, ExplorationCursorMode expected)
    {
        var control = Assert.Single(GameMenuInput.Resolve(Catalog()),
            item => item.Action == action);

        var command = Assert.IsType<ExplorationCommand>(GameMenuInput.CommandFor(control));

        Assert.Equal(ExplorationCommandKind.SelectCursorMode, command.Kind);
        Assert.Equal(expected, command.CursorMode);
    }

    [Fact]
    public void RoutesReturnAndLeavesUnevidencedDestinationsInert()
    {
        var controls = GameMenuInput.Resolve(Catalog());
        var returnControl = Assert.Single(controls,
            item => item.Action == GameMenuAction.ReturnToGame);
        Assert.Equal(ExplorationCommandKind.Escape,
            GameMenuInput.CommandFor(returnControl)!.Kind);

        var pending = new[] { GameMenuAction.ExitToDos, GameMenuAction.LoadSave };
        Assert.All(controls.Where(item => pending.Contains(item.Action)),
            item => Assert.Null(GameMenuInput.CommandFor(item)));
    }

    [Fact]
    public void RoutesCollapsePartyToLeaderOnlyDisplay()
    {
        var control = Assert.Single(GameMenuInput.Resolve(Catalog()),
            item => item.Action == GameMenuAction.CollapseParty);

        var command = Assert.IsType<ExplorationCommand>(
            GameMenuInput.CommandFor(control));

        Assert.Equal(ExplorationCommandKind.ShowLeaderOnly, command.Kind);
    }

    [Fact]
    public void RoutesCenterOnLeaderWithRuntimeActorContext()
    {
        var control = Assert.Single(GameMenuInput.Resolve(Catalog()),
            item => item.Action == GameMenuAction.CenterOnLeader);

        Assert.Null(GameMenuInput.CommandFor(control));
        var command = Assert.IsType<ExplorationCommand>(
            GameMenuInput.CommandFor(control, 1_192, 1_476));

        Assert.Equal(ExplorationCommandKind.CenterCamera, command.Kind);
        Assert.Equal((1_192, 1_476),
            (command.TargetWorldX, command.TargetWorldY));
    }

    [Fact]
    public void RejectsAControlWhoseMappedImageDoesNotMatch()
    {
        var catalog = Catalog();
        var buttons = catalog.Buttons.ToArray();
        buttons[0] = buttons[0] with { ImageResourceNumber = 999 };

        Assert.Throws<InvalidDataException>(() => GameMenuInput.Resolve(
            catalog with { Buttons = buttons }));
    }

    [Fact]
    public void RejectsADuplicatedMappedButtonReference()
    {
        var catalog = Catalog();
        var children = catalog.Windows[0].Children.ToArray();
        children[^1] = children[0];
        var window = catalog.Windows[0] with { Children = children };

        var error = Assert.Throws<InvalidDataException>(() => GameMenuInput.Resolve(
            catalog with { Windows = [window] }));

        Assert.Contains("exactly one BUTN #10300", error.Message);
    }

    private static PackedUiCatalog Catalog()
    {
        var children = OriginalContent.GameMenuButtons.Select(asset =>
            new UiChildReference("BUTN", asset.ButtonResourceNumber,
                checked((short)asset.X), checked((short)asset.Y))).ToArray();
        var window = new UiWindowResource(OriginalContent.GameMenuWindowResourceNumber, 0,
            checked((ushort)OriginalContent.GameMenuLayer.FrameWidth),
            checked((ushort)OriginalContent.GameMenuLayer.FrameHeight), children);
        var buttons = OriginalContent.GameMenuButtons.Select(asset =>
            new UiButtonResource(asset.ButtonResourceNumber,
                checked((ushort)asset.FrameWidth), checked((ushort)asset.FrameHeight),
                asset.ImageResourceNumber, 0)).ToArray();
        return new([window], buttons, [], []);
    }
}
