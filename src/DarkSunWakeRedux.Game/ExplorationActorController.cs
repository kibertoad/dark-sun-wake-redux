using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public readonly record struct ExplorationActorVisualSnapshot(
    GridPoint Anchor,
    GridPoint TargetAnchor,
    long ProgressTicks,
    long StepTicks);

public sealed class ExplorationActorController
{
    // PLACEHOLDER: RULE-EXPLORE-005 - how long the original takes for one step of a walk is unknown;
    // DEV-EXPLORE-002 records these values as the rebuild's own.
    public static readonly TimeSpan DefaultStepInterval = TimeSpan.FromMilliseconds(125);
    public const int MaximumStepsPerUpdate = 4;

    private readonly ExplorationActorMovementSession _movement;
    private readonly RegionTerrainGrid _terrain;
    private readonly long _stepTicks;
    private long _accumulatedTicks;

    public ExplorationActorController(
        RegionTerrainGrid terrain,
        ExplorationOccupancySession occupancy,
        int occupantId,
        TimeSpan? stepInterval = null)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        ArgumentNullException.ThrowIfNull(occupancy);
        var interval = stepInterval ?? DefaultStepInterval;
        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(stepInterval),
                "The actor step interval must be positive.");

        var occupancySnapshot = occupancy.Snapshot();
        if (occupancySnapshot.Width != terrain.Width ||
            occupancySnapshot.Height != terrain.Height)
            throw new ArgumentException(
                "Actor occupancy dimensions must match the terrain grid.",
                nameof(occupancy));
        _movement = new(occupancy, occupantId);
        _terrain = terrain;
        _stepTicks = interval.Ticks;
    }

    public ExplorationActorMovementSnapshot Snapshot() => _movement.Snapshot();

    public ExplorationActorVisualSnapshot VisualSnapshot()
    {
        var movement = _movement.Snapshot();
        var target = movement.RemainingSteps.Count == 0
            ? movement.Position
            : movement.RemainingSteps[0];
        var progress = target == movement.Position
            ? 0
            : Math.Min(_accumulatedTicks, _stepTicks);
        return new(movement.Position, target, progress, _stepTicks);
    }

    public ExplorationActorMovementTransition? PlanAt(
        ExplorationSnapshot exploration,
        int logicalX,
        int logicalY)
    {
        var command = ExplorationTerrainRoutePlanner.CommandAt(
            _terrain, exploration, logicalX, logicalY);
        if (command is null) return null;
        _accumulatedTicks = 0;
        return _movement.Execute(command);
    }

    public IReadOnlyList<ExplorationActorMovementTransition> Advance(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed),
                "Elapsed movement time cannot be negative.");
        if (_movement.Snapshot().RemainingSteps.Count == 0)
        {
            _accumulatedTicks = 0;
            return [];
        }

        _accumulatedTicks = SaturatingAdd(_accumulatedTicks, elapsed.Ticks);
        var due = Math.Min(_accumulatedTicks / _stepTicks, MaximumStepsPerUpdate);
        var transitions = new List<ExplorationActorMovementTransition>(checked((int)due));
        for (var index = 0; index < due; index++)
        {
            _accumulatedTicks -= _stepTicks;
            var transition = _movement.Execute(ExplorationMoveCommand.Advance());
            transitions.Add(transition);
            if (transition.After.RemainingSteps.Count == 0)
            {
                _accumulatedTicks = 0;
                break;
            }
        }
        return transitions.AsReadOnly();
    }

    private static long SaturatingAdd(long accumulatedTicks, long elapsedTicks) =>
        elapsedTicks > long.MaxValue - accumulatedTicks
            ? long.MaxValue
            : accumulatedTicks + elapsedTicks;
}
