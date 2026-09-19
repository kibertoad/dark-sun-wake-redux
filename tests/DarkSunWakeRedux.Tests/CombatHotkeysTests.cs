using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class CombatHotkeysTests
{
    [Theory]
    [InlineData(Keys.G, CombatCommandKind.Guard)]
    [InlineData(Keys.N, CombatCommandKind.TargetNext)]
    [InlineData(Keys.P, CombatCommandKind.TargetPrevious)]
    [InlineData(Keys.Q, CombatCommandKind.EndTurn)]
    [InlineData(Keys.W, CombatCommandKind.Wait)]
    [InlineData(Keys.Space, CombatCommandKind.DisableComputerControl)]
    public void MapsEachManualCombatHotkey(Keys key, CombatCommandKind expected)
    {
        var command = Assert.IsType<CombatCommand>(CombatHotkeys.Resolve(new(key), new()));

        Assert.Equal(expected, command.Kind);
    }

    [Fact]
    public void EmitsOnlyOnTheRisingEdge()
    {
        Assert.Null(CombatHotkeys.Resolve(new(Keys.G), new(Keys.G)));
        Assert.Null(CombatHotkeys.Resolve(new(), new()));
    }

    [Fact]
    public void SimultaneousKeysUseStableManualBindingOrder()
    {
        var command = Assert.IsType<CombatCommand>(
            CombatHotkeys.Resolve(new(Keys.W, Keys.G), new()));

        Assert.Equal(CombatCommandKind.Guard, command.Kind);
    }

    [Fact]
    public void RejectsUnknownCoreCommands() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new CombatCommand(
            (CombatCommandKind)999));
}
