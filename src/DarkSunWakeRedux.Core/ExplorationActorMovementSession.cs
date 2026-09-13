using System.Collections.ObjectModel;

namespace DarkSunWakeRedux.Core;

public sealed record ExplorationActorMovementSnapshot(
    int OccupantId,
    GridPoint Position,
    GridPoint? Destination,
    IReadOnlyList<GridPoint> RemainingSteps);

public sealed record ExplorationActorMovementTransition(
    ExplorationActorMovementSnapshot Before,
    ExplorationMoveCommand Command,
    ExplorationActorMovementSnapshot After,
    IReadOnlyList<ExplorationMoveEvent> MovementEvents,
    IReadOnlyList<ExplorationOccupancyEvent> OccupancyEvents,
    bool Applied);

public sealed class ExplorationActorMovementSession
{
    private readonly ExplorationOccupancySession _occupancy;
    private readonly int _occupantId;
    private readonly ExplorationMovementSession _movement;
    private IReadOnlyList<ExplorationOccupancyEvent> _stepOccupancyEvents = [];

    public ExplorationActorMovementSession(
        ExplorationOccupancySession occupancy,
        int occupantId)
    {
        ArgumentNullException.ThrowIfNull(occupancy);
        var placement = occupancy.PlacementOf(occupantId) ??
            throw new ArgumentException(
                "The actor must have an occupancy placement before movement begins.",
                nameof(occupantId));
        _occupancy = occupancy;
        _occupantId = occupantId;
        var occupancySnapshot = occupancy.Snapshot();
        _movement = new(occupancySnapshot.Width, occupancySnapshot.Height,
            placement.Anchor, occupancy.AnchorPassability(occupantId), CommitStep);
    }

    public ExplorationActorMovementSnapshot Snapshot()
    {
        EnsureSynchronized();
        var movement = _movement.Snapshot();
        return new(_occupantId, movement.Position, movement.Destination,
            Array.AsReadOnly(movement.RemainingSteps.ToArray()));
    }

    public ExplorationActorMovementTransition Execute(ExplorationMoveCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var before = Snapshot();
        _stepOccupancyEvents = [];
        var movement = _movement.Execute(command);
        var after = Snapshot();
        return new(before, command, after, movement.Events, _stepOccupancyEvents,
            Changed(before, after));
    }

    private bool CommitStep(GridPoint anchor)
    {
        var transition = _occupancy.Execute(
            ExplorationOccupancyCommand.MoveTo(_occupantId, anchor));
        _stepOccupancyEvents = new ReadOnlyCollection<ExplorationOccupancyEvent>(
            transition.Events.ToArray());
        return transition.Applied;
    }

    private void EnsureSynchronized()
    {
        var placement = _occupancy.PlacementOf(_occupantId);
        if (placement is null)
            throw new InvalidOperationException(
                "The moving actor no longer has an occupancy placement.");
        if (placement.Anchor != _movement.Snapshot().Position)
            throw new InvalidOperationException(
                "The moving actor's route and occupancy anchors are out of sync.");
    }

    private static bool Changed(
        ExplorationActorMovementSnapshot before,
        ExplorationActorMovementSnapshot after) =>
        before.Position != after.Position || before.Destination != after.Destination ||
        !before.RemainingSteps.SequenceEqual(after.RemainingSteps);
}
