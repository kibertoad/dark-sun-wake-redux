using System.Collections.ObjectModel;

namespace DarkSunWakeRedux.Core;

public sealed class GridFootprint
{
    private readonly ReadOnlyCollection<GridPoint> _offsets;

    public GridFootprint(IEnumerable<GridPoint> offsets)
    {
        ArgumentNullException.ThrowIfNull(offsets);
        var ordered = offsets.Distinct().OrderBy(point => point.Y).ThenBy(point => point.X)
            .ToArray();
        if (ordered.Length == 0)
            throw new ArgumentException("A grid footprint must contain at least one cell.",
                nameof(offsets));
        if (ordered.Length > GridPathfinder.MaximumCellCount)
            throw new ArgumentException(
                $"A grid footprint cannot exceed {GridPathfinder.MaximumCellCount} cells.",
                nameof(offsets));
        _offsets = Array.AsReadOnly(ordered);
    }

    public static GridFootprint SingleCell { get; } = new([new(0, 0)]);

    public IReadOnlyList<GridPoint> Offsets => _offsets;
}

public enum ExplorationOccupancyCommandKind
{
    Place,
    Move,
    Remove
}

public sealed record ExplorationOccupancyCommand(
    ExplorationOccupancyCommandKind Kind,
    int OccupantId,
    GridPoint? Anchor = null,
    GridFootprint? Footprint = null)
{
    public static ExplorationOccupancyCommand PlaceAt(
        int occupantId, GridPoint anchor, GridFootprint footprint) =>
        new(ExplorationOccupancyCommandKind.Place, occupantId, anchor, footprint);

    public static ExplorationOccupancyCommand MoveTo(int occupantId, GridPoint anchor) =>
        new(ExplorationOccupancyCommandKind.Move, occupantId, anchor);

    public static ExplorationOccupancyCommand Remove(int occupantId) =>
        new(ExplorationOccupancyCommandKind.Remove, occupantId);
}

public enum ExplorationOccupancyEventKind
{
    Placed,
    Moved,
    Removed,
    Rejected
}

public enum ExplorationOccupancyRejection
{
    None,
    AlreadyPlaced,
    NotPlaced,
    OutsideGrid,
    TerrainBlocked,
    Occupied
}

public sealed record ExplorationOccupancyEvent(
    ExplorationOccupancyEventKind Kind,
    int OccupantId,
    GridPoint? Anchor,
    ExplorationOccupancyRejection Rejection = ExplorationOccupancyRejection.None);

public sealed record ExplorationOccupantPlacement(
    int OccupantId,
    GridPoint Anchor,
    GridFootprint Footprint);

public sealed record ExplorationOccupancySnapshot(
    int Width,
    int Height,
    IReadOnlyList<ExplorationOccupantPlacement> Placements);

public sealed record ExplorationOccupancyTransition(
    ExplorationOccupancySnapshot Before,
    ExplorationOccupancyCommand Command,
    ExplorationOccupancySnapshot After,
    IReadOnlyList<ExplorationOccupancyEvent> Events,
    bool Applied);

public sealed class ExplorationOccupancySession
{
    private readonly int _width;
    private readonly int _height;
    private readonly Func<GridPoint, bool> _isTerrainPassable;
    private readonly Dictionary<int, ExplorationOccupantPlacement> _placements = [];
    private readonly Dictionary<GridPoint, int> _occupiedCells = [];

    public ExplorationOccupancySession(
        int width,
        int height,
        Func<GridPoint, bool> isTerrainPassable)
    {
        ArgumentNullException.ThrowIfNull(isTerrainPassable);
        if (width <= 0 || height <= 0 ||
            (long)width * height > GridPathfinder.MaximumCellCount)
            throw new ArgumentOutOfRangeException(nameof(width),
                $"Occupancy grid must contain 1 through {GridPathfinder.MaximumCellCount} cells.");
        _width = width;
        _height = height;
        _isTerrainPassable = isTerrainPassable;
    }

    public ExplorationOccupancySnapshot Snapshot() =>
        new(_width, _height, Array.AsReadOnly(
            _placements.Values.OrderBy(item => item.OccupantId).ToArray()));

    public int? OccupantAt(GridPoint point) =>
        _occupiedCells.TryGetValue(point, out var occupantId) ? occupantId : null;

    public bool IsCellOpen(GridPoint point, int? exceptOccupantId = null) =>
        InBounds(point) && _isTerrainPassable(point) &&
        (!_occupiedCells.TryGetValue(point, out var occupantId) ||
         occupantId == exceptOccupantId);

    public bool CanOccupy(int occupantId, GridPoint anchor)
    {
        if (!_placements.TryGetValue(occupantId, out var placement)) return false;
        return RejectionAt(occupantId, anchor, placement.Footprint) ==
            ExplorationOccupancyRejection.None;
    }

    public Func<GridPoint, bool> AnchorPassability(int occupantId)
    {
        if (!_placements.ContainsKey(occupantId))
            throw new ArgumentException("The occupant must be placed before routing it.",
                nameof(occupantId));
        return anchor => CanOccupy(occupantId, anchor);
    }

    public ExplorationOccupancyTransition Execute(ExplorationOccupancyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Validate(command);
        var before = Snapshot();
        IReadOnlyList<ExplorationOccupancyEvent> events = command.Kind switch
        {
            ExplorationOccupancyCommandKind.Place => Place(command),
            ExplorationOccupancyCommandKind.Move => Move(command),
            ExplorationOccupancyCommandKind.Remove => Remove(command),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Kind,
                "Unknown exploration occupancy command kind.")
        };
        var after = Snapshot();
        return new(before, command, after, events, Changed(before, after));
    }

    private IReadOnlyList<ExplorationOccupancyEvent> Place(
        ExplorationOccupancyCommand command)
    {
        if (_placements.ContainsKey(command.OccupantId))
            return [Rejected(command, ExplorationOccupancyRejection.AlreadyPlaced)];
        var rejection = RejectionAt(
            command.OccupantId, command.Anchor!.Value, command.Footprint!);
        if (rejection != ExplorationOccupancyRejection.None)
            return [Rejected(command, rejection)];
        Add(new(command.OccupantId, command.Anchor.Value, command.Footprint!));
        return [new(ExplorationOccupancyEventKind.Placed, command.OccupantId,
            command.Anchor)];
    }

    private IReadOnlyList<ExplorationOccupancyEvent> Move(
        ExplorationOccupancyCommand command)
    {
        if (!_placements.TryGetValue(command.OccupantId, out var current))
            return [Rejected(command, ExplorationOccupancyRejection.NotPlaced)];
        if (current.Anchor == command.Anchor) return [];
        var rejection = RejectionAt(
            command.OccupantId, command.Anchor!.Value, current.Footprint);
        if (rejection != ExplorationOccupancyRejection.None)
            return [Rejected(command, rejection)];
        RemoveCells(current);
        Add(new(command.OccupantId, command.Anchor.Value, current.Footprint));
        return [new(ExplorationOccupancyEventKind.Moved, command.OccupantId,
            command.Anchor)];
    }

    private IReadOnlyList<ExplorationOccupancyEvent> Remove(
        ExplorationOccupancyCommand command)
    {
        if (!_placements.Remove(command.OccupantId, out var current))
            return [Rejected(command, ExplorationOccupancyRejection.NotPlaced)];
        RemoveCells(current);
        return [new(ExplorationOccupancyEventKind.Removed, command.OccupantId,
            current.Anchor)];
    }

    private ExplorationOccupancyRejection RejectionAt(
        int occupantId, GridPoint anchor, GridFootprint footprint)
    {
        foreach (var offset in footprint.Offsets)
        {
            var x = (long)anchor.X + offset.X;
            var y = (long)anchor.Y + offset.Y;
            if (x < 0 || y < 0 || x >= _width || y >= _height)
                return ExplorationOccupancyRejection.OutsideGrid;
            var point = new GridPoint((int)x, (int)y);
            if (!_isTerrainPassable(point))
                return ExplorationOccupancyRejection.TerrainBlocked;
            if (_occupiedCells.TryGetValue(point, out var existing) && existing != occupantId)
                return ExplorationOccupancyRejection.Occupied;
        }
        return ExplorationOccupancyRejection.None;
    }

    private void Add(ExplorationOccupantPlacement placement)
    {
        _placements[placement.OccupantId] = placement;
        foreach (var offset in placement.Footprint.Offsets)
            _occupiedCells.Add(new(
                placement.Anchor.X + offset.X, placement.Anchor.Y + offset.Y),
                placement.OccupantId);
    }

    private void RemoveCells(ExplorationOccupantPlacement placement)
    {
        foreach (var offset in placement.Footprint.Offsets)
            _occupiedCells.Remove(new(
                placement.Anchor.X + offset.X, placement.Anchor.Y + offset.Y));
    }

    private bool InBounds(GridPoint point) =>
        point.X >= 0 && point.Y >= 0 && point.X < _width && point.Y < _height;

    private static ExplorationOccupancyEvent Rejected(
        ExplorationOccupancyCommand command, ExplorationOccupancyRejection rejection) =>
        new(ExplorationOccupancyEventKind.Rejected, command.OccupantId,
            command.Anchor, rejection);

    private static bool Changed(
        ExplorationOccupancySnapshot before, ExplorationOccupancySnapshot after) =>
        !before.Placements.SequenceEqual(after.Placements);

    private static void Validate(ExplorationOccupancyCommand command)
    {
        var valid = command.OccupantId >= 0 && command.Kind switch
        {
            ExplorationOccupancyCommandKind.Place =>
                command.Anchor is not null && command.Footprint is not null,
            ExplorationOccupancyCommandKind.Move =>
                command.Anchor is not null && command.Footprint is null,
            ExplorationOccupancyCommandKind.Remove =>
                command.Anchor is null && command.Footprint is null,
            _ => false
        };
        if (!valid)
            throw new ArgumentException(
                "The exploration occupancy command payload does not match its kind.",
                nameof(command));
    }
}
