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

    [Theory]
    [InlineData(2, 0)]
    [InlineData(0, 0)]
    public void RejectsInvalidScrollDeltas(int x, int y) =>
        Assert.ThrowsAny<ArgumentException>(() => Session().Execute(
            ExplorationCommand.Scroll(x, y)));

    [Theory]
    [InlineData(ExplorationView.ViewCharacter)]
    [InlineData(ExplorationView.ViewInventory)]
    [InlineData(ExplorationView.CastSpellsOrUsePsionics)]
    [InlineData(ExplorationView.CurrentSpellEffects)]
    [InlineData(ExplorationView.OverheadMap)]
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
    public void GameMenuModeSelectionReturnsToWorld()
    {
        var session = Session();
        session.Execute(ExplorationCommand.Open(ExplorationView.GameMenu));

        session.Execute(ExplorationCommand.SelectMode(ExplorationCursorMode.Look));

        Assert.Equal(ExplorationCursorMode.Look, session.Snapshot().CursorMode);
        Assert.Equal(ExplorationView.World, session.Snapshot().View);
    }

    [Fact]
    public void WorldCommandsAreSuspendedWhileMenuIsOpen()
    {
        var session = Session();
        session.Execute(ExplorationCommand.Open(ExplorationView.ViewInventory));

        Assert.False(session.Execute(ExplorationCommand.Scroll(1, 0)).Applied);
        Assert.False(session.Execute(new(ExplorationCommandKind.CycleCursorMode)).Applied);
        Assert.False(session.Execute(new(ExplorationCommandKind.ShowExpandedParty)).Applied);
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
            CursorMode: (ExplorationCursorMode)99)
    };

    private static ExplorationSession Session() =>
        new(2048, 1568, 320, 200, 1024, 1368);
}
