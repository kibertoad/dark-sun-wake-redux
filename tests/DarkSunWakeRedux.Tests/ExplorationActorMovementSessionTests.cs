using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationActorMovementSessionTests
{
    [Fact]
    public void PlansAndAdvancesRouteAndOccupancyInLockstep()
    {
        var (occupancy, actor) = Actor(new(0, 0));

        var planned = actor.Execute(ExplorationMoveCommand.Plan(new(2, 0)));
        var advanced = actor.Execute(ExplorationMoveCommand.Advance());

        Assert.True(planned.Applied);
        Assert.Empty(planned.OccupancyEvents);
        Assert.Equal(new GridPoint(1, 0), advanced.After.Position);
        Assert.Equal(new GridPoint(1, 0), occupancy.PlacementOf(1)!.Anchor);
        Assert.Equal(ExplorationMoveEventKind.StepTaken,
            Assert.Single(advanced.MovementEvents).Kind);
        Assert.Equal(ExplorationOccupancyEventKind.Moved,
            Assert.Single(advanced.OccupancyEvents).Kind);
    }

    [Fact]
    public void WholeFootprintShapesTheRouteAndMovesAtomically()
    {
        var occupancy = Occupancy();
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(0, 0), new([new(0, 0), new(1, 0)])));
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            2, new(2, 0), GridFootprint.SingleCell));
        var actor = new ExplorationActorMovementSession(occupancy, 1);

        var planned = actor.Execute(ExplorationMoveCommand.Plan(new(2, 1)));
        var advanced = actor.Execute(ExplorationMoveCommand.Advance());

        Assert.Equal([new(0, 1), new(1, 1), new(2, 1)],
            planned.After.RemainingSteps);
        Assert.Equal(new GridPoint(0, 1), advanced.After.Position);
        Assert.Equal(1, occupancy.OccupantAt(new(0, 1)));
        Assert.Equal(1, occupancy.OccupantAt(new(1, 1)));
        Assert.Equal(2, occupancy.OccupantAt(new(2, 0)));
    }

    [Fact]
    public void NewBlockerInterruptsWithoutMovingOccupancy()
    {
        var (occupancy, actor) = Actor(new(0, 0));
        actor.Execute(ExplorationMoveCommand.Plan(new(2, 0)));
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            2, new(1, 0), GridFootprint.SingleCell));

        var interrupted = actor.Execute(ExplorationMoveCommand.Advance());

        Assert.Equal(new GridPoint(0, 0), interrupted.After.Position);
        Assert.Equal(new GridPoint(0, 0), occupancy.PlacementOf(1)!.Anchor);
        Assert.Equal(ExplorationMoveEventKind.RouteInterrupted,
            Assert.Single(interrupted.MovementEvents).Kind);
        Assert.Empty(interrupted.OccupancyEvents);
    }

    [Fact]
    public void ReplanningAndCancellationDoNotMutateOccupancy()
    {
        var (occupancy, actor) = Actor(new(0, 0));
        actor.Execute(ExplorationMoveCommand.Plan(new(3, 0)));
        actor.Execute(ExplorationMoveCommand.Plan(new(0, 3)));

        var cancelled = actor.Execute(ExplorationMoveCommand.Cancel());

        Assert.Equal(new GridPoint(0, 0), occupancy.PlacementOf(1)!.Anchor);
        Assert.Equal(ExplorationMoveEventKind.RouteCancelled,
            Assert.Single(cancelled.MovementEvents).Kind);
        Assert.Empty(cancelled.OccupancyEvents);
    }

    [Fact]
    public void SnapshotsAreImmutableAndCommandTracesAreRepeatable()
    {
        var (_, first) = Actor(new(0, 0));
        var (_, second) = Actor(new(0, 0));
        var commands = new[]
        {
            ExplorationMoveCommand.Plan(new(2, 1)),
            ExplorationMoveCommand.Advance(),
            ExplorationMoveCommand.Advance()
        };
        var snapshot = first.Snapshot();

        var firstTrace = commands.Select(first.Execute).ToArray();
        var secondTrace = commands.Select(second.Execute).ToArray();

        Assert.Equal(firstTrace.Length, secondTrace.Length);
        for (var index = 0; index < firstTrace.Length; index++)
        {
            Assert.Equal(firstTrace[index].After.OccupantId,
                secondTrace[index].After.OccupantId);
            Assert.Equal(firstTrace[index].After.Position,
                secondTrace[index].After.Position);
            Assert.Equal(firstTrace[index].After.Destination,
                secondTrace[index].After.Destination);
            Assert.Equal(firstTrace[index].After.RemainingSteps,
                secondTrace[index].After.RemainingSteps);
            Assert.Equal(firstTrace[index].MovementEvents,
                secondTrace[index].MovementEvents);
            Assert.Equal(firstTrace[index].OccupancyEvents,
                secondTrace[index].OccupancyEvents);
        }
        Assert.Empty(snapshot.RemainingSteps);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<GridPoint>)snapshot.RemainingSteps).Clear());
    }

    [Fact]
    public void DetectsMissingAndExternallyMovedOccupancy()
    {
        var occupancy = Occupancy();
        Assert.Throws<ArgumentException>(() =>
            new ExplorationActorMovementSession(occupancy, 1));
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(0, 0), GridFootprint.SingleCell));
        var actor = new ExplorationActorMovementSession(occupancy, 1);
        occupancy.Execute(ExplorationOccupancyCommand.MoveTo(1, new(1, 0)));

        Assert.Throws<InvalidOperationException>(actor.Snapshot);
        Assert.Throws<InvalidOperationException>(() =>
            actor.Execute(ExplorationMoveCommand.Advance()));
    }

    private static (ExplorationOccupancySession Occupancy,
        ExplorationActorMovementSession Actor) Actor(GridPoint anchor)
    {
        var occupancy = Occupancy();
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, anchor, GridFootprint.SingleCell));
        return (occupancy, new(occupancy, 1));
    }

    private static ExplorationOccupancySession Occupancy() =>
        new(5, 5, _ => true);
}
