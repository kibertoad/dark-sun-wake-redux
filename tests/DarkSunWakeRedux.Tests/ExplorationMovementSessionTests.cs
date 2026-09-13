using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationMovementSessionTests
{
    [Fact]
    public void PlansAndAdvancesOneDeterministicSemanticStepAtATime()
    {
        var session = Session(new(0, 0));

        var planned = session.Execute(ExplorationMoveCommand.Plan(new(2, 1)));

        Assert.True(planned.Applied);
        Assert.Equal(new GridPoint(2, 1), planned.After.Destination);
        Assert.Equal([new(1, 1), new(2, 1)], planned.After.RemainingSteps);
        Assert.Equal([ExplorationMoveEventKind.RoutePlanned],
            planned.Events.Select(item => item.Kind));

        var first = session.Execute(ExplorationMoveCommand.Advance());
        Assert.Equal(new GridPoint(1, 1), first.After.Position);
        Assert.Equal([ExplorationMoveEventKind.StepTaken],
            first.Events.Select(item => item.Kind));

        var second = session.Execute(ExplorationMoveCommand.Advance());
        Assert.Equal(new GridPoint(2, 1), second.After.Position);
        Assert.Null(second.After.Destination);
        Assert.Empty(second.After.RemainingSteps);
        Assert.Equal(
            [ExplorationMoveEventKind.StepTaken, ExplorationMoveEventKind.RouteCompleted],
            second.Events.Select(item => item.Kind));
    }

    [Fact]
    public void ReplanningReplacesTheRemainingRouteDeterministically()
    {
        var session = Session(new(0, 0));
        session.Execute(ExplorationMoveCommand.Plan(new(4, 0)));
        session.Execute(ExplorationMoveCommand.Advance());

        var replanned = session.Execute(ExplorationMoveCommand.Plan(new(1, 3)));

        Assert.Equal(new GridPoint(1, 0), replanned.Before.Position);
        Assert.Equal(new GridPoint(1, 3), replanned.After.Destination);
        Assert.Equal([new(1, 1), new(1, 2), new(1, 3)],
            replanned.After.RemainingSteps);
    }

    [Fact]
    public void RejectedPlanPreservesAnExistingRouteAtomically()
    {
        var blocked = new HashSet<GridPoint> { new(4, 4) };
        var session = Session(new(0, 0), blocked);
        session.Execute(ExplorationMoveCommand.Plan(new(3, 0)));
        var before = session.Snapshot();

        var rejected = session.Execute(ExplorationMoveCommand.Plan(new(4, 4)));

        Assert.False(rejected.Applied);
        Assert.Equal(ExplorationMoveEventKind.RouteRejected,
            Assert.Single(rejected.Events).Kind);
        Assert.Equal(before.Position, rejected.After.Position);
        Assert.Equal(before.Destination, rejected.After.Destination);
        Assert.Equal(before.RemainingSteps, rejected.After.RemainingSteps);
    }

    [Fact]
    public void NewlyBlockedStepInterruptsWithoutEnteringIt()
    {
        var blocked = new HashSet<GridPoint>();
        var session = Session(new(0, 0), blocked);
        session.Execute(ExplorationMoveCommand.Plan(new(2, 0)));
        blocked.Add(new(1, 0));

        var interrupted = session.Execute(ExplorationMoveCommand.Advance());

        Assert.True(interrupted.Applied);
        Assert.Equal(new GridPoint(0, 0), interrupted.After.Position);
        Assert.Null(interrupted.After.Destination);
        Assert.Empty(interrupted.After.RemainingSteps);
        Assert.Equal(ExplorationMoveEventKind.RouteInterrupted,
            Assert.Single(interrupted.Events).Kind);
    }

    [Fact]
    public void NewlyBlockedDiagonalCornerInterruptsTheRoute()
    {
        var blocked = new HashSet<GridPoint>();
        var session = Session(new(0, 0), blocked);
        session.Execute(ExplorationMoveCommand.Plan(new(1, 1)));
        blocked.Add(new(1, 0));

        var interrupted = session.Execute(ExplorationMoveCommand.Advance());

        Assert.Equal(new GridPoint(0, 0), interrupted.After.Position);
        Assert.Equal(ExplorationMoveEventKind.RouteInterrupted,
            Assert.Single(interrupted.Events).Kind);
    }

    [Fact]
    public void RejectedStepCommitInterruptsWithoutAdvancingRouteState()
    {
        var session = new ExplorationMovementSession(
            5, 5, new(0, 0), _ => true, _ => false);
        session.Execute(ExplorationMoveCommand.Plan(new(2, 0)));

        var interrupted = session.Execute(ExplorationMoveCommand.Advance());

        Assert.Equal(new GridPoint(0, 0), interrupted.After.Position);
        Assert.Null(interrupted.After.Destination);
        Assert.Empty(interrupted.After.RemainingSteps);
        Assert.Equal(ExplorationMoveEventKind.RouteInterrupted,
            Assert.Single(interrupted.Events).Kind);
    }

    [Fact]
    public void CancellationAndIdleAdvanceAreIdempotent()
    {
        var session = Session(new(0, 0));

        Assert.False(session.Execute(ExplorationMoveCommand.Advance()).Applied);
        Assert.False(session.Execute(ExplorationMoveCommand.Cancel()).Applied);
        session.Execute(ExplorationMoveCommand.Plan(new(2, 0)));

        var cancelled = session.Execute(ExplorationMoveCommand.Cancel());

        Assert.True(cancelled.Applied);
        Assert.Equal(ExplorationMoveEventKind.RouteCancelled,
            Assert.Single(cancelled.Events).Kind);
        Assert.False(session.Execute(ExplorationMoveCommand.Cancel()).Applied);
    }

    [Fact]
    public void ZeroLengthPlanCompletesWithoutChangingState()
    {
        var session = Session(new(2, 2));

        var transition = session.Execute(ExplorationMoveCommand.Plan(new(2, 2)));

        Assert.False(transition.Applied);
        Assert.Equal(
            [ExplorationMoveEventKind.RoutePlanned, ExplorationMoveEventKind.RouteCompleted],
            transition.Events.Select(item => item.Kind));
    }

    [Fact]
    public void SnapshotsDoNotChangeAfterLaterCommands()
    {
        var session = Session(new(0, 0));
        session.Execute(ExplorationMoveCommand.Plan(new(2, 0)));
        var snapshot = session.Snapshot();

        session.Execute(ExplorationMoveCommand.Advance());

        Assert.Equal(new GridPoint(0, 0), snapshot.Position);
        Assert.Equal([new(1, 0), new(2, 0)], snapshot.RemainingSteps);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<GridPoint>)snapshot.RemainingSteps).Clear());
    }

    [Fact]
    public void RejectsInvalidStartAndCommandPayloads()
    {
        Assert.Throws<ArgumentException>(() =>
            Session(new(1, 1), [new(1, 1)]));
        var session = Session(new(0, 0));
        Assert.Throws<ArgumentException>(() => session.Execute(
            new(ExplorationMoveCommandKind.PlanRoute)));
        Assert.Throws<ArgumentException>(() => session.Execute(
            new(ExplorationMoveCommandKind.AdvanceRoute, new(1, 1))));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Execute(ExplorationMoveCommand.Plan(new(5, 0))));
    }

    private static ExplorationMovementSession Session(
        GridPoint start, HashSet<GridPoint>? blocked = null) =>
        new(5, 5, start, point => blocked?.Contains(point) != true);
}
