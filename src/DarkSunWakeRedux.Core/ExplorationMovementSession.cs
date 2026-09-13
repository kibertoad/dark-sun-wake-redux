namespace DarkSunWakeRedux.Core;

public enum ExplorationMoveCommandKind
{
    PlanRoute,
    AdvanceRoute,
    CancelRoute
}

public sealed record ExplorationMoveCommand(
    ExplorationMoveCommandKind Kind,
    GridPoint? Destination = null)
{
    public static ExplorationMoveCommand Plan(GridPoint destination) =>
        new(ExplorationMoveCommandKind.PlanRoute, destination);

    public static ExplorationMoveCommand Advance() =>
        new(ExplorationMoveCommandKind.AdvanceRoute);

    public static ExplorationMoveCommand Cancel() =>
        new(ExplorationMoveCommandKind.CancelRoute);
}

public enum ExplorationMoveEventKind
{
    RoutePlanned,
    RouteRejected,
    StepTaken,
    RouteCompleted,
    RouteInterrupted,
    RouteCancelled
}

public sealed record ExplorationMoveEvent(
    ExplorationMoveEventKind Kind,
    GridPoint Point);

public sealed record ExplorationMoveSnapshot(
    GridPoint Position,
    GridPoint? Destination,
    IReadOnlyList<GridPoint> RemainingSteps);

public sealed record ExplorationMoveTransition(
    ExplorationMoveSnapshot Before,
    ExplorationMoveCommand Command,
    ExplorationMoveSnapshot After,
    IReadOnlyList<ExplorationMoveEvent> Events,
    bool Applied);

public sealed class ExplorationMovementSession
{
    private readonly int _width;
    private readonly int _height;
    private readonly Func<GridPoint, bool> _isPassable;
    private GridPoint _position;
    private GridPoint? _destination;
    private readonly List<GridPoint> _remainingSteps = [];

    public ExplorationMovementSession(
        int width,
        int height,
        GridPoint position,
        Func<GridPoint, bool> isPassable)
    {
        ArgumentNullException.ThrowIfNull(isPassable);
        var validation = GridPathfinder.FindPath(width, height, position, position, isPassable);
        if (!validation.Found)
            throw new ArgumentException("The initial movement position must be passable.",
                nameof(position));
        _width = width;
        _height = height;
        _position = position;
        _isPassable = isPassable;
    }

    public ExplorationMoveSnapshot Snapshot() =>
        new(_position, _destination, _remainingSteps.ToArray());

    public ExplorationMoveTransition Execute(ExplorationMoveCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Validate(command);
        var before = Snapshot();
        IReadOnlyList<ExplorationMoveEvent> events = command.Kind switch
        {
            ExplorationMoveCommandKind.PlanRoute => Plan(command.Destination!.Value),
            ExplorationMoveCommandKind.AdvanceRoute => Advance(),
            ExplorationMoveCommandKind.CancelRoute => Cancel(),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Kind,
                "Unknown exploration movement command kind.")
        };
        var after = Snapshot();
        return new(before, command, after, events, Changed(before, after));
    }

    private IReadOnlyList<ExplorationMoveEvent> Plan(GridPoint destination)
    {
        var route = GridPathfinder.FindPath(
            _width, _height, _position, destination, _isPassable);
        if (!route.Found)
            return [new(ExplorationMoveEventKind.RouteRejected, destination)];

        _remainingSteps.Clear();
        _remainingSteps.AddRange(route.Points.Skip(1));
        _destination = _remainingSteps.Count == 0 ? null : destination;
        var events = new List<ExplorationMoveEvent>
        {
            new(ExplorationMoveEventKind.RoutePlanned, destination)
        };
        if (_remainingSteps.Count == 0)
            events.Add(new(ExplorationMoveEventKind.RouteCompleted, destination));
        return events;
    }

    private IReadOnlyList<ExplorationMoveEvent> Advance()
    {
        if (_remainingSteps.Count == 0) return [];
        var next = _remainingSteps[0];
        if (!CanEnter(next))
        {
            _remainingSteps.Clear();
            _destination = null;
            return [new(ExplorationMoveEventKind.RouteInterrupted, next)];
        }

        _position = next;
        _remainingSteps.RemoveAt(0);
        var events = new List<ExplorationMoveEvent>
        {
            new(ExplorationMoveEventKind.StepTaken, next)
        };
        if (_remainingSteps.Count == 0)
        {
            _destination = null;
            events.Add(new(ExplorationMoveEventKind.RouteCompleted, next));
        }
        return events;
    }

    private IReadOnlyList<ExplorationMoveEvent> Cancel()
    {
        if (_remainingSteps.Count == 0) return [];
        var cancelledDestination = _destination!.Value;
        _remainingSteps.Clear();
        _destination = null;
        return [new(ExplorationMoveEventKind.RouteCancelled, cancelledDestination)];
    }

    private bool CanEnter(GridPoint next)
    {
        var deltaX = next.X - _position.X;
        var deltaY = next.Y - _position.Y;
        if (Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1 || deltaX == 0 && deltaY == 0)
            throw new InvalidOperationException("The planned route contains a non-adjacent step.");
        if (!_isPassable(next)) return false;
        return deltaX == 0 || deltaY == 0 ||
            _isPassable(new(_position.X + deltaX, _position.Y)) &&
            _isPassable(new(_position.X, _position.Y + deltaY));
    }

    private static void Validate(ExplorationMoveCommand command)
    {
        var valid = command.Kind switch
        {
            ExplorationMoveCommandKind.PlanRoute => command.Destination is not null,
            ExplorationMoveCommandKind.AdvanceRoute or ExplorationMoveCommandKind.CancelRoute =>
                command.Destination is null,
            _ => false
        };
        if (!valid)
            throw new ArgumentException(
                "The exploration movement command payload does not match its kind.",
                nameof(command));
    }

    private static bool Changed(
        ExplorationMoveSnapshot before, ExplorationMoveSnapshot after) =>
        before.Position != after.Position || before.Destination != after.Destination ||
        !before.RemainingSteps.SequenceEqual(after.RemainingSteps);
}
