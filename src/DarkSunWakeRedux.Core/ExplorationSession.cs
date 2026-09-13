namespace DarkSunWakeRedux.Core;

public enum ExplorationCursorMode
{
    Walk,
    Attack,
    Look
}

public enum PartyDisplayMode
{
    LeaderOnly,
    Expanded
}

public enum ExplorationCommandKind
{
    ScrollCamera,
    CycleCursorMode,
    ShowLeaderOnly,
    ShowExpandedParty
}

public sealed record ExplorationCommand(ExplorationCommandKind Kind, int DeltaX = 0, int DeltaY = 0)
{
    public static ExplorationCommand Scroll(int deltaX, int deltaY) =>
        new(ExplorationCommandKind.ScrollCamera, deltaX, deltaY);
}

public sealed record ExplorationSnapshot(
    int CameraX,
    int CameraY,
    ExplorationCursorMode CursorMode,
    PartyDisplayMode PartyDisplay);

public sealed record ExplorationTransition(
    ExplorationSnapshot Before,
    ExplorationCommand Command,
    ExplorationSnapshot After,
    bool Applied);

public sealed class ExplorationSession
{
    private readonly int _maximumCameraX;
    private readonly int _maximumCameraY;
    private ExplorationSnapshot _snapshot;

    public ExplorationSession(
        int worldWidth,
        int worldHeight,
        int viewportWidth,
        int viewportHeight,
        int cameraX,
        int cameraY)
    {
        if (worldWidth <= 0 || worldHeight <= 0 || viewportWidth <= 0 || viewportHeight <= 0 ||
            viewportWidth > worldWidth || viewportHeight > worldHeight)
            throw new ArgumentOutOfRangeException(nameof(viewportWidth),
                "Exploration world and viewport dimensions must be positive and the viewport must fit.");
        _maximumCameraX = worldWidth - viewportWidth;
        _maximumCameraY = worldHeight - viewportHeight;
        if (cameraX < 0 || cameraY < 0 || cameraX > _maximumCameraX || cameraY > _maximumCameraY)
            throw new ArgumentOutOfRangeException(nameof(cameraX),
                "The initial exploration camera must fit within the world.");
        _snapshot = new(cameraX, cameraY, ExplorationCursorMode.Walk, PartyDisplayMode.LeaderOnly);
    }

    public ExplorationSnapshot Snapshot() => _snapshot;

    public ExplorationTransition Execute(ExplorationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var before = _snapshot;
        _snapshot = command.Kind switch
        {
            ExplorationCommandKind.ScrollCamera => Scroll(command.DeltaX, command.DeltaY),
            ExplorationCommandKind.CycleCursorMode => _snapshot with
            {
                CursorMode = _snapshot.CursorMode switch
                {
                    ExplorationCursorMode.Walk => ExplorationCursorMode.Attack,
                    ExplorationCursorMode.Attack => ExplorationCursorMode.Look,
                    _ => ExplorationCursorMode.Walk
                }
            },
            ExplorationCommandKind.ShowLeaderOnly => _snapshot with
                { PartyDisplay = PartyDisplayMode.LeaderOnly },
            ExplorationCommandKind.ShowExpandedParty => _snapshot with
                { PartyDisplay = PartyDisplayMode.Expanded },
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Kind,
                "Unknown exploration command kind.")
        };
        return new(before, command, _snapshot, before != _snapshot);
    }

    private ExplorationSnapshot Scroll(int deltaX, int deltaY)
    {
        if (deltaX is < -1 or > 1 || deltaY is < -1 or > 1 || deltaX == 0 && deltaY == 0)
            throw new ArgumentOutOfRangeException(nameof(deltaX),
                "A camera scroll command must move one pixel in at least one axis.");
        return _snapshot with
        {
            CameraX = Math.Clamp(_snapshot.CameraX + deltaX, 0, _maximumCameraX),
            CameraY = Math.Clamp(_snapshot.CameraY + deltaY, 0, _maximumCameraY)
        };
    }
}
