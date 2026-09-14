using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationCursorFeedbackTests
{
    [Theory]
    [InlineData(ExplorationCursorMode.Walk, true, false, false,
        ExplorationCursorVisual.Walk)]
    [InlineData(ExplorationCursorMode.Walk, false, false, false,
        ExplorationCursorVisual.CannotWalk)]
    [InlineData(ExplorationCursorMode.Attack, false, true, false,
        ExplorationCursorVisual.MeleeAttack)]
    [InlineData(ExplorationCursorMode.Attack, false, false, false,
        ExplorationCursorVisual.CannotMeleeAttack)]
    [InlineData(ExplorationCursorMode.Look, false, false, true,
        ExplorationCursorVisual.Look)]
    [InlineData(ExplorationCursorMode.Look, false, false, false,
        ExplorationCursorVisual.CannotLook)]
    public void ResolvesObservedValidAndInvalidModePairs(
        ExplorationCursorMode mode,
        bool walkReachable,
        bool meleeTarget,
        bool lookTarget,
        ExplorationCursorVisual expected)
    {
        var snapshot = new ExplorationSnapshot(0, 0, mode,
            PartyDisplayMode.LeaderOnly, ExplorationView.World);

        Assert.Equal(expected, ExplorationCursorFeedback.Resolve(
            snapshot, walkReachable, meleeTarget, lookTarget));
    }

    [Fact]
    public void OverlaysUseTheNeutralWalkPointer()
    {
        var snapshot = new ExplorationSnapshot(0, 0, ExplorationCursorMode.Attack,
            PartyDisplayMode.LeaderOnly, ExplorationView.GameMenu);

        Assert.Equal(ExplorationCursorVisual.Walk,
            ExplorationCursorFeedback.Resolve(snapshot, false, false, false));
    }

    [Fact]
    public void EveryVisualMapsToOneExactOwnedAsset()
    {
        Assert.Equal(10, OriginalContent.ExplorationCursorAssets.Count);
        Assert.Equal(10, OriginalContent.ExplorationCursorAssets
            .Select(asset => asset.ResourceNumber).Distinct().Count());
        foreach (var visual in Enum.GetValues<ExplorationCursorVisual>())
        {
            var asset = Assert.Single(OriginalContent.ExplorationCursorAssets,
                item => item.Name == ExplorationCursorFeedback.AssetName(visual));
            Assert.Equal("ICON", asset.Tag);
            Assert.Equal(1, asset.FrameCount);
            Assert.InRange(asset.ResourceNumber, 19101U, 19110U);
        }
    }
}
