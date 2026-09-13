using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationActorControllerTests
{
    [Fact]
    public void PlansFromLogicalClickAndAdvancesAtFixedIntervals()
    {
        var controller = Controller();

        var planned = controller.PlanAt(Snapshot(), 56, 24);
        Assert.NotNull(planned);
        Assert.Equal(new GridPoint(3, 1), planned.After.Destination);
        Assert.Empty(controller.Advance(TimeSpan.FromMilliseconds(124)));

        var first = Assert.Single(controller.Advance(TimeSpan.FromMilliseconds(1)));
        Assert.Equal(new GridPoint(2, 1), first.After.Position);
        var second = Assert.Single(controller.Advance(TimeSpan.FromMilliseconds(125)));
        Assert.Equal(new GridPoint(3, 1), second.After.Position);
        Assert.Null(second.After.Destination);
    }

    [Fact]
    public void AccumulationIsIndependentOfFrameChunking()
    {
        var first = Controller();
        var second = Controller();
        first.PlanAt(Snapshot(), 56, 24);
        second.PlanAt(Snapshot(), 56, 24);

        first.Advance(TimeSpan.FromMilliseconds(50));
        first.Advance(TimeSpan.FromMilliseconds(75));
        second.Advance(TimeSpan.FromMilliseconds(125));

        var firstSnapshot = first.Snapshot();
        var secondSnapshot = second.Snapshot();
        Assert.Equal(firstSnapshot.Position, secondSnapshot.Position);
        Assert.Equal(firstSnapshot.Destination, secondSnapshot.Destination);
        Assert.Equal(firstSnapshot.RemainingSteps, secondSnapshot.RemainingSteps);
    }

    [Fact]
    public void CapsCatchUpWorkWithoutDiscardingTheBacklog()
    {
        var controller = Controller();
        controller.PlanAt(Snapshot(), 168, 24);

        var first = controller.Advance(TimeSpan.FromSeconds(1));
        var second = controller.Advance(TimeSpan.Zero);

        Assert.Equal(ExplorationActorController.MaximumStepsPerUpdate, first.Count);
        Assert.Equal(ExplorationActorController.MaximumStepsPerUpdate, second.Count);
        Assert.Equal(new GridPoint(9, 1), controller.Snapshot().Position);
    }

    [Fact]
    public void ReplanningResetsPartialCadenceAndUsesLiveActorPosition()
    {
        var controller = Controller();
        controller.PlanAt(Snapshot(), 56, 24);
        controller.Advance(TimeSpan.FromMilliseconds(100));

        controller.PlanAt(Snapshot(), 24, 56);

        Assert.Empty(controller.Advance(TimeSpan.FromMilliseconds(25)));
        Assert.Equal(new GridPoint(1, 1), controller.Snapshot().Position);
        Assert.Single(controller.Advance(TimeSpan.FromMilliseconds(100)));
    }

    [Theory]
    [InlineData(ExplorationCursorMode.Look, ExplorationView.World)]
    [InlineData(ExplorationCursorMode.Walk, ExplorationView.GameMenu)]
    public void IgnoresClicksOutsideActiveWalkWorld(
        ExplorationCursorMode mode,
        ExplorationView view)
    {
        Assert.Null(Controller().PlanAt(Snapshot(mode, view), 56, 24));
    }

    [Fact]
    public void RequiresPlacedActorAndValidElapsedTime()
    {
        var terrain = Terrain();
        var emptyOccupancy = Occupancy(terrain);
        Assert.Throws<ArgumentException>(() =>
            new ExplorationActorController(terrain, emptyOccupancy, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Controller().Advance(TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public void SharedOccupancyInterruptsAControllerBeforeBlockedStep()
    {
        var terrain = Terrain();
        var occupancy = Occupancy(terrain);
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(1, 1), GridFootprint.SingleCell));
        var controller = new ExplorationActorController(terrain, occupancy, 1);
        controller.PlanAt(Snapshot(), 56, 24);
        occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            2, new(2, 1), GridFootprint.SingleCell));

        var transition = Assert.Single(controller.Advance(
            ExplorationActorController.DefaultStepInterval));

        Assert.Equal(ExplorationMoveEventKind.RouteInterrupted,
            Assert.Single(transition.MovementEvents).Kind);
        Assert.Equal(new GridPoint(1, 1), transition.After.Position);
    }

    private static ExplorationActorController Controller(
        params (int X, int Y)[] blocked)
    {
        var terrain = Terrain(blocked);
        var occupancy = Occupancy(terrain);
        var placed = occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(1, 1), GridFootprint.SingleCell));
        Assert.True(placed.Applied);
        return new(terrain, occupancy, 1);
    }

    private static ExplorationOccupancySession Occupancy(RegionTerrainGrid terrain) =>
        new(terrain.Width, terrain.Height,
            point => terrain.IsTerrainOpen(point.X, point.Y));

    private static ExplorationSnapshot Snapshot(
        ExplorationCursorMode mode = ExplorationCursorMode.Walk,
        ExplorationView view = ExplorationView.World) =>
        new(0, 0, mode, PartyDisplayMode.LeaderOnly, view);

    private static RegionTerrainGrid Terrain(params (int X, int Y)[] blocked)
    {
        var geometry = new byte[GffRegion.MapByteCount];
        foreach (var point in blocked)
            geometry[point.Y * GffRegion.TileColumns + point.X] =
                RegionTerrainGrid.BlockingMask;
        return new(new PackedRegion(50, "Synthetic", [],
            new byte[GffRegion.MapByteCount], geometry, new Dictionary<byte,
                IndexedImageFrame>(), []));
    }
}
