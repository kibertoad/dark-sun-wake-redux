using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationSessionTests
{
    [Fact]
    public void StartsInWalkModeWithOnlyTheLeaderShown()
    {
        var session = Session();

        Assert.Equal(new ExplorationSnapshot(1024, 1368,
            ExplorationCursorMode.Walk, PartyDisplayMode.LeaderOnly,
            ExplorationView.World), session.Snapshot());
    }

    [Fact]
    public void ScrollsOnePixelAndClampsAtWorldEdges()
    {
        var session = Session();

        Assert.True(session.Execute(ExplorationCommand.Scroll(-1, -1)).Applied);
        Assert.Equal((1023, 1367), (session.Snapshot().CameraX, session.Snapshot().CameraY));
        Assert.True(session.Execute(ExplorationCommand.Scroll(0, 1)).Applied);
        Assert.False(session.Execute(ExplorationCommand.Scroll(0, 1)).Applied);

        var topLeft = new ExplorationSession(2048, 1568, 320, 200, 0, 0);
        Assert.False(topLeft.Execute(ExplorationCommand.Scroll(-1, -1)).Applied);
    }

    [Fact]
    public void PansByBoundedLogicalPointerDeltaAndClampsAtWorldEdges()
    {
        var session = Session();

        var panned = session.Execute(ExplorationCommand.Pan(-24, 15));

        Assert.True(panned.Applied);
        Assert.Equal((1000, 1368), (panned.After.CameraX, panned.After.CameraY));
        Assert.False(session.Execute(ExplorationCommand.Pan(0, 15)).Applied);
    }

    [Fact]
    public void PanClampsExtremeDeltasWithoutWrappingOnLargeWorlds()
    {
        var maximum = int.MaxValue - 1;
        var session = new ExplorationSession(int.MaxValue, int.MaxValue, 1, 1,
            maximum, maximum);

        var transition = session.Execute(ExplorationCommand.Pan(
            int.MaxValue, int.MaxValue));

        Assert.False(transition.Applied);
        Assert.Equal((maximum, maximum),
            (transition.After.CameraX, transition.After.CameraY));
    }

    [Fact]
    public void CyclesCursorModesInManualOrder()
    {
        var session = Session();
        var cycle = new ExplorationCommand(ExplorationCommandKind.CycleCursorMode);

        Assert.Equal(ExplorationCursorMode.Attack, session.Execute(cycle).After.CursorMode);
        Assert.Equal(ExplorationCursorMode.Look, session.Execute(cycle).After.CursorMode);
        Assert.Equal(ExplorationCursorMode.Walk, session.Execute(cycle).After.CursorMode);
    }

    [Fact]
    public void ChangesPartyDisplayIdempotently()
    {
        var session = Session();

        Assert.True(session.Execute(new(ExplorationCommandKind.ShowExpandedParty)).Applied);
        Assert.Equal(PartyDisplayMode.Expanded, session.Snapshot().PartyDisplay);
        Assert.False(session.Execute(new(ExplorationCommandKind.ShowExpandedParty)).Applied);
        Assert.True(session.Execute(new(ExplorationCommandKind.ShowLeaderOnly)).Applied);
    }

    [Theory]
    [InlineData(0, 50, -1, 0)]
    [InlineData(319, 50, 1, 0)]
    [InlineData(50, 0, 0, -1)]
    [InlineData(50, 199, 0, 1)]
    [InlineData(0, 0, -1, -1)]
    public void MapsLogicalCanvasEdgesToCameraCommands(
        int x, int y, int expectedX, int expectedY)
    {
        var command = Assert.IsType<ExplorationCommand>(ExplorationInput.ScrollAtEdge(x, y));

        Assert.Equal((expectedX, expectedY), (command.DeltaX, command.DeltaY));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(318, 198)]
    [InlineData(-1, 50)]
    [InlineData(320, 50)]
    public void DoesNotScrollAwayFromCanvasEdges(int x, int y) =>
        Assert.Null(ExplorationInput.ScrollAtEdge(x, y));

    [Fact]
    public void MapsExpandedViewportEdgesToTheSameScrollCommands()
    {
        Assert.Equal((-1, 0), Deltas(ExplorationInput.ScrollAtEdge(0, 50, 356, 200)!));
        Assert.Equal((1, 1), Deltas(ExplorationInput.ScrollAtEdge(355, 199, 356, 200)!));
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(0, 0)]
    public void RejectsInvalidScrollDeltas(int x, int y) =>
        Assert.ThrowsAny<ArgumentException>(() => Session().Execute(
            ExplorationCommand.Scroll(x, y)));

    [Theory]
    [InlineData(2049, 0)]
    [InlineData(0, 1569)]
    [InlineData(0, 0)]
    public void RejectsInvalidPanDeltas(int x, int y) =>
        Assert.ThrowsAny<ArgumentException>(() => Session().Execute(
            ExplorationCommand.Pan(x, y)));

    [Theory]
    [InlineData(ExplorationView.ViewCharacter)]
    [InlineData(ExplorationView.ViewInventory)]
    [InlineData(ExplorationView.CastSpellsOrUsePsionics)]
    [InlineData(ExplorationView.CurrentSpellEffects)]
    [InlineData(ExplorationView.OverheadMap)]
    [InlineData(ExplorationView.Preferences)]
    [InlineData(ExplorationView.GameMenu)]
    public void OpensEveryDocumentedExplorationView(ExplorationView view)
    {
        var session = Session();

        Assert.True(session.Execute(ExplorationCommand.Open(view)).Applied);
        Assert.Equal(view, session.Snapshot().View);
        Assert.True(session.Execute(new(ExplorationCommandKind.Escape)).Applied);
        Assert.Equal(ExplorationView.World, session.Snapshot().View);
    }

    [Fact]
    public void EscapeFromWorldRequestsExit()
    {
        var session = Session();

        session.Execute(new(ExplorationCommandKind.Escape));

        Assert.Equal(ExplorationView.ExitRequested, session.Snapshot().View);
    }

    [Fact]
    public void ExplicitExitRequestLeavesTheActiveMenuForExit()
    {
        var session = Session();
        session.Execute(ExplorationCommand.Open(ExplorationView.GameMenu));

        var transition = session.Execute(ExplorationCommand.RequestExit());

        Assert.True(transition.Applied);
        Assert.Equal(ExplorationView.ExitRequested, transition.After.View);
    }

    [Fact]
    public void GameMenuModeSelectionReturnsToWorld()
    {
        var session = Session();
        session.Execute(ExplorationCommand.Open(ExplorationView.GameMenu));

        session.Execute(ExplorationCommand.SelectMode(ExplorationCursorMode.Look));

        Assert.Equal(ExplorationCursorMode.Look, session.Snapshot().CursorMode);
        Assert.Equal(ExplorationView.World, session.Snapshot().View);
    }

    [Fact]
    public void CentersCameraOnAWorldPointAndReturnsFromGameMenu()
    {
        var session = Session();
        session.Execute(ExplorationCommand.Open(ExplorationView.GameMenu));

        var transition = session.Execute(ExplorationCommand.CenterOn(1_192, 1_476));

        Assert.True(transition.Applied);
        Assert.Equal((1_032, 1_368, ExplorationView.World),
            (transition.After.CameraX, transition.After.CameraY, transition.After.View));
    }

    [Fact]
    public void CollapsePartyReturnsFromGameMenuInLeaderOnlyMode()
    {
        var session = Session();
        session.Execute(new(ExplorationCommandKind.ShowExpandedParty));
        session.Execute(ExplorationCommand.Open(ExplorationView.GameMenu));

        var collapsed = session.Execute(
            new(ExplorationCommandKind.ShowLeaderOnly));

        Assert.True(collapsed.Applied);
        Assert.Equal((PartyDisplayMode.LeaderOnly, ExplorationView.World),
            (collapsed.After.PartyDisplay, collapsed.After.View));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(2_047, 1_567, 1_728, 1_368)]
    public void ClampsCenteredCameraAtWorldEdges(
        int worldX,
        int worldY,
        int expectedCameraX,
        int expectedCameraY)
    {
        var centered = Session().Execute(
            ExplorationCommand.CenterOn(worldX, worldY)).After;

        Assert.Equal((expectedCameraX, expectedCameraY),
            (centered.CameraX, centered.CameraY));
    }

    [Fact]
    public void WorldCommandsAreSuspendedWhileMenuIsOpen()
    {
        var session = Session();
        session.Execute(ExplorationCommand.Open(ExplorationView.ViewInventory));

        Assert.False(session.Execute(ExplorationCommand.Scroll(1, 0)).Applied);
        Assert.False(session.Execute(ExplorationCommand.Pan(10, 10)).Applied);
        Assert.False(session.Execute(new(ExplorationCommandKind.CycleCursorMode)).Applied);
        Assert.False(session.Execute(new(ExplorationCommandKind.ShowExpandedParty)).Applied);
        Assert.False(session.Execute(ExplorationCommand.CenterOn(100, 100)).Applied);
    }

    [Theory]
    [MemberData(nameof(InvalidCommands))]
    public void RejectsMismatchedCommandPayloads(ExplorationCommand command) =>
        Assert.Throws<ArgumentException>(() => Session().Execute(command));

    public static TheoryData<ExplorationCommand> InvalidCommands => new()
    {
        new ExplorationCommand(ExplorationCommandKind.OpenView),
        ExplorationCommand.Open(ExplorationView.World),
        new ExplorationCommand(ExplorationCommandKind.Escape,
            View: ExplorationView.GameMenu),
        new ExplorationCommand(ExplorationCommandKind.SelectCursorMode,
            CursorMode: (ExplorationCursorMode)99),
        new ExplorationCommand(ExplorationCommandKind.CenterCamera,
            TargetWorldX: 100),
        ExplorationCommand.CenterOn(-1, 0),
        ExplorationCommand.CenterOn(2_048, 0)
    };

    private static ExplorationSession Session() =>
        new(2048, 1568, 320, 200, 1024, 1368);

    private static (int X, int Y) Deltas(ExplorationCommand command) =>
        (command.DeltaX, command.DeltaY);
}
