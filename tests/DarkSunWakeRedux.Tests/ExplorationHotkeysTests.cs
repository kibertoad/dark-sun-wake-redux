using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationHotkeysTests
{
    [Theory]
    [InlineData(Keys.Tab, ExplorationView.GameMenu)]
    [InlineData(Keys.V, ExplorationView.ViewCharacter)]
    [InlineData(Keys.I, ExplorationView.ViewInventory)]
    [InlineData(Keys.C, ExplorationView.CastSpellsOrUsePsionics)]
    [InlineData(Keys.U, ExplorationView.CastSpellsOrUsePsionics)]
    [InlineData(Keys.E, ExplorationView.CurrentSpellEffects)]
    [InlineData(Keys.O, ExplorationView.OverheadMap)]
    public void MapsDocumentedMenuHotkeys(Keys key, ExplorationView expected)
    {
        var command = Assert.IsType<ExplorationCommand>(
            ExplorationHotkeys.Resolve(new(key), new()));

        Assert.Equal(ExplorationCommandKind.OpenView, command.Kind);
        Assert.Equal(expected, command.View);
    }

    [Theory]
    [InlineData(Keys.Escape, ExplorationCommandKind.Escape)]
    [InlineData(Keys.D5, ExplorationCommandKind.ShowExpandedParty)]
    [InlineData(Keys.D6, ExplorationCommandKind.ShowLeaderOnly)]
    public void MapsDocumentedActionHotkeys(Keys key, ExplorationCommandKind expected)
    {
        var command = Assert.IsType<ExplorationCommand>(
            ExplorationHotkeys.Resolve(new(key), new()));

        Assert.Equal(expected, command.Kind);
    }

    [Fact]
    public void EmitsOnlyOnTheRisingEdge()
    {
        Assert.Null(ExplorationHotkeys.Resolve(new(Keys.V), new(Keys.V)));
        Assert.Null(ExplorationHotkeys.Resolve(new(), new()));
    }

    [Fact]
    public void SimultaneousKeysUseStableBindingOrder()
    {
        var command = Assert.IsType<ExplorationCommand>(
            ExplorationHotkeys.Resolve(new(Keys.I, Keys.Tab), new()));

        Assert.Equal(ExplorationView.GameMenu, command.View);
    }
}
