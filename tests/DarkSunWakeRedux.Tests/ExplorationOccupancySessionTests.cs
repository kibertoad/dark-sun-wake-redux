using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationOccupancySessionTests
{
    [Fact]
    public void PlacesCanonicalMultiCellFootprintAndQueriesItsCells()
    {
        var session = Session();
        var footprint = new GridFootprint([new(1, 0), new(0, 0), new(1, 0)]);

        var transition = session.Execute(
            ExplorationOccupancyCommand.PlaceAt(7, new(2, 3), footprint));

        Assert.True(transition.Applied);
        Assert.Equal(ExplorationOccupancyEventKind.Placed,
            Assert.Single(transition.Events).Kind);
        Assert.Equal([new(0, 0), new(1, 0)], footprint.Offsets);
        Assert.Equal(7, session.OccupantAt(new(2, 3)));
        Assert.Equal(7, session.OccupantAt(new(3, 3)));
        Assert.Null(session.OccupantAt(new(4, 3)));
    }

    [Theory]
    [InlineData(ExplorationOccupancyRejection.OutsideGrid, 4, 1)]
    [InlineData(ExplorationOccupancyRejection.TerrainBlocked, 1, 1)]
    public void RejectsInvalidPlacementAtomically(
        ExplorationOccupancyRejection expected, int x, int y)
    {
        var session = Session(blocked: [new(2, 1)]);
        var before = session.Snapshot();

        var transition = session.Execute(ExplorationOccupancyCommand.PlaceAt(
            7, new(x, y), new([new(0, 0), new(1, 0)])));

        Assert.False(transition.Applied);
        Assert.Equal(expected, Assert.Single(transition.Events).Rejection);
        Assert.Equal(before.Placements, transition.After.Placements);
        Assert.Null(session.OccupantAt(new(x, y)));
    }

    [Fact]
    public void RejectsOverlapWithoutChangingEitherPlacement()
    {
        var session = Session();
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            3, new(1, 1), new([new(0, 0), new(1, 0)])));
        var before = session.Snapshot();

        var transition = session.Execute(ExplorationOccupancyCommand.PlaceAt(
            4, new(2, 1), GridFootprint.SingleCell));

        Assert.False(transition.Applied);
        Assert.Equal(ExplorationOccupancyRejection.Occupied,
            Assert.Single(transition.Events).Rejection);
        Assert.Equal(before.Placements, transition.After.Placements);
        Assert.Equal(3, session.OccupantAt(new(2, 1)));
    }

    [Fact]
    public void MovesAtomicallyAndMayOverlapItsOwnPreviousCells()
    {
        var session = Session();
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            3, new(1, 1), new([new(0, 0), new(1, 0)])));

        var transition = session.Execute(
            ExplorationOccupancyCommand.MoveTo(3, new(2, 1)));

        Assert.True(transition.Applied);
        Assert.Equal(ExplorationOccupancyEventKind.Moved,
            Assert.Single(transition.Events).Kind);
        Assert.Null(session.OccupantAt(new(1, 1)));
        Assert.Equal(3, session.OccupantAt(new(2, 1)));
        Assert.Equal(3, session.OccupantAt(new(3, 1)));
    }

    [Fact]
    public void RejectedMovePreservesEveryPreviouslyOccupiedCell()
    {
        var session = Session(blocked: [new(3, 1)]);
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            3, new(1, 1), new([new(0, 0), new(1, 0)])));

        var transition = session.Execute(
            ExplorationOccupancyCommand.MoveTo(3, new(2, 1)));

        Assert.False(transition.Applied);
        Assert.Equal(ExplorationOccupancyRejection.TerrainBlocked,
            Assert.Single(transition.Events).Rejection);
        Assert.Equal(3, session.OccupantAt(new(1, 1)));
        Assert.Equal(3, session.OccupantAt(new(2, 1)));
        Assert.Null(session.OccupantAt(new(3, 1)));
    }

    [Fact]
    public void RemovesPlacementAndReportsUnknownOccupants()
    {
        var session = Session();
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            3, new(1, 1), GridFootprint.SingleCell));

        var removed = session.Execute(ExplorationOccupancyCommand.Remove(3));
        var repeated = session.Execute(ExplorationOccupancyCommand.Remove(3));
        var missingMove = session.Execute(
            ExplorationOccupancyCommand.MoveTo(9, new(2, 2)));

        Assert.True(removed.Applied);
        Assert.Equal(ExplorationOccupancyEventKind.Removed,
            Assert.Single(removed.Events).Kind);
        Assert.Equal(ExplorationOccupancyRejection.NotPlaced,
            Assert.Single(repeated.Events).Rejection);
        Assert.Equal(ExplorationOccupancyRejection.NotPlaced,
            Assert.Single(missingMove.Events).Rejection);
    }

    [Fact]
    public void AnchorPassabilityUsesTheWholeFootprintAndLiveOccupancy()
    {
        var session = Session();
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(0, 0), new([new(0, 0), new(1, 0)])));
        var passable = session.AnchorPassability(1);

        Assert.True(passable(new(1, 0)));
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            2, new(2, 0), GridFootprint.SingleCell));
        Assert.False(passable(new(1, 0)));
        session.Execute(ExplorationOccupancyCommand.MoveTo(2, new(4, 4)));
        Assert.True(passable(new(1, 0)));
        Assert.False(passable(new(4, 0)));
    }

    [Fact]
    public void AnchorPassabilityFeedsDeterministicRoutePlanning()
    {
        var occupancy = Session();
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(0, 0), GridFootprint.SingleCell));
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            2, new(1, 0), GridFootprint.SingleCell));

        var movement = new ExplorationMovementSession(
            5, 5, new(0, 0), occupancy.AnchorPassability(1));
        var route = movement.Execute(ExplorationMoveCommand.Plan(new(2, 0)));

        Assert.Equal([new(0, 1), new(1, 1), new(2, 1), new(2, 0)],
            route.After.RemainingSteps);
    }

    [Fact]
    public void NewlyOccupiedCellInterruptsAComposedMovementSession()
    {
        var occupancy = Session();
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(0, 0), GridFootprint.SingleCell));
        var movement = new ExplorationMovementSession(
            5, 5, new(0, 0), occupancy.AnchorPassability(1));
        movement.Execute(ExplorationMoveCommand.Plan(new(2, 0)));
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            2, new(1, 0), GridFootprint.SingleCell));

        var interrupted = movement.Execute(ExplorationMoveCommand.Advance());

        Assert.Equal(ExplorationMoveEventKind.RouteInterrupted,
            Assert.Single(interrupted.Events).Kind);
        Assert.Equal(new GridPoint(0, 0), interrupted.After.Position);
    }

    [Fact]
    public void SnapshotsAreOrderedAndUnaffectedByLaterChanges()
    {
        var session = Session();
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            9, new(1, 1), GridFootprint.SingleCell));
        session.Execute(ExplorationOccupancyCommand.PlaceAt(
            2, new(2, 2), GridFootprint.SingleCell));
        var snapshot = session.Snapshot();

        session.Execute(ExplorationOccupancyCommand.Remove(2));

        Assert.Equal([2, 9], snapshot.Placements.Select(item => item.OccupantId));
        Assert.Equal(2, snapshot.Placements.Count);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<ExplorationOccupantPlacement>)snapshot.Placements).Clear());
    }

    [Fact]
    public void RejectsInvalidDimensionsFootprintsAndCommandPayloads()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ExplorationOccupancySession(0, 5, _ => true));
        Assert.Throws<ArgumentException>(() => new GridFootprint([]));
        var session = Session();
        Assert.Throws<ArgumentException>(() => session.Execute(
            new(ExplorationOccupancyCommandKind.Place, 1, new(1, 1))));
        Assert.Throws<ArgumentException>(() => session.Execute(
            new(ExplorationOccupancyCommandKind.Remove, 1, new(1, 1))));
        Assert.Throws<ArgumentException>(() => session.Execute(
            ExplorationOccupancyCommand.Remove(-1)));
        Assert.Throws<ArgumentException>(() => session.AnchorPassability(1));
    }

    private static ExplorationOccupancySession Session(
        IReadOnlyCollection<GridPoint>? blocked = null) =>
        new(5, 5, point => blocked?.Contains(point) != true);
}
