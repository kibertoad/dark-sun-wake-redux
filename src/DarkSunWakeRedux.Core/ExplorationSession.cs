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

public enum ExplorationView
{
    World,
    ViewCharacter,
    ViewInventory,
    CastSpellsOrUsePsionics,
    CurrentSpellEffects,
    OverheadMap,
    Preferences,
    GameMenu,
    ExitRequested
}

public enum ExplorationCommandKind
{
    ScrollCamera,
    CycleCursorMode,
    ShowLeaderOnly,
    ShowExpandedParty,
    OpenView,
    SelectCursorMode,
    CenterCamera,
    Escape
}

public sealed record ExplorationCommand(
    ExplorationCommandKind Kind,
    int DeltaX = 0,
    int DeltaY = 0,
    ExplorationView? View = null,
    ExplorationCursorMode? CursorMode = null,
    int? TargetWorldX = null,
    int? TargetWorldY = null)
{
    public static ExplorationCommand Scroll(int deltaX, int deltaY) =>
        new(ExplorationCommandKind.ScrollCamera, deltaX, deltaY);

    public static ExplorationCommand Open(ExplorationView view) =>
        new(ExplorationCommandKind.OpenView, View: view);

    public static ExplorationCommand SelectMode(ExplorationCursorMode mode) =>
        new(ExplorationCommandKind.SelectCursorMode, CursorMode: mode);

    public static ExplorationCommand CenterOn(int worldX, int worldY) =>
        new(ExplorationCommandKind.CenterCamera,
            TargetWorldX: worldX, TargetWorldY: worldY);
}

public sealed record ExplorationSnapshot(
    int CameraX,
    int CameraY,
    ExplorationCursorMode CursorMode,
    PartyDisplayMode PartyDisplay,
    ExplorationView View);

public sealed record ExplorationTransition(
    ExplorationSnapshot Before,
    ExplorationCommand Command,
    ExplorationSnapshot After,
    bool Applied);

public sealed class ExplorationSession
{
    private readonly int _maximumCameraX;
    private readonly int _maximumCameraY;
    private readonly int _worldWidth;
    private readonly int _worldHeight;
    private readonly int _viewportWidth;
    private readonly int _viewportHeight;
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
        _worldWidth = worldWidth;
        _worldHeight = worldHeight;
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
        if (cameraX < 0 || cameraY < 0 || cameraX > _maximumCameraX || cameraY > _maximumCameraY)
            throw new ArgumentOutOfRangeException(nameof(cameraX),
                "The initial exploration camera must fit within the world.");
        _snapshot = new(cameraX, cameraY, ExplorationCursorMode.Walk,
            PartyDisplayMode.LeaderOnly, ExplorationView.World);
    }

    public ExplorationSnapshot Snapshot() => _snapshot;

    public ExplorationTransition Execute(ExplorationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Validate(command);
        var before = _snapshot;
        _snapshot = command.Kind switch
        {
            ExplorationCommandKind.ScrollCamera when IsWorldActive =>
                Scroll(command.DeltaX, command.DeltaY),
            ExplorationCommandKind.CycleCursorMode when IsWorldActive => _snapshot with
            {
                CursorMode = _snapshot.CursorMode switch
                {
                    ExplorationCursorMode.Walk => ExplorationCursorMode.Attack,
                    ExplorationCursorMode.Attack => ExplorationCursorMode.Look,
                    _ => ExplorationCursorMode.Walk
                }
            },
            ExplorationCommandKind.ShowLeaderOnly when
                _snapshot.View is ExplorationView.World or ExplorationView.GameMenu =>
                _snapshot with
                {
                    PartyDisplay = PartyDisplayMode.LeaderOnly,
                    View = ExplorationView.World
                },
            ExplorationCommandKind.ShowExpandedParty when IsWorldActive => _snapshot with
                { PartyDisplay = PartyDisplayMode.Expanded },
            ExplorationCommandKind.OpenView => _snapshot with { View = command.View!.Value },
            ExplorationCommandKind.SelectCursorMode => _snapshot with
            {
                CursorMode = command.CursorMode!.Value,
                View = ExplorationView.World
            },
            ExplorationCommandKind.CenterCamera when
                _snapshot.View is ExplorationView.World or ExplorationView.GameMenu =>
                CenterOn(command.TargetWorldX!.Value, command.TargetWorldY!.Value),
            ExplorationCommandKind.Escape => _snapshot with
            {
                View = IsWorldActive ? ExplorationView.ExitRequested : ExplorationView.World
            },
            ExplorationCommandKind.ScrollCamera or ExplorationCommandKind.CycleCursorMode or
                ExplorationCommandKind.ShowLeaderOnly or ExplorationCommandKind.ShowExpandedParty or
                ExplorationCommandKind.CenterCamera =>
                _snapshot,
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Kind,
                "Unknown exploration command kind.")
        };
        return new(before, command, _snapshot, before != _snapshot);
    }

    private bool IsWorldActive => _snapshot.View == ExplorationView.World;

    private void Validate(ExplorationCommand command)
    {
        var hasMovement = command.DeltaX != 0 || command.DeltaY != 0;
        var hasTarget = command.TargetWorldX is not null || command.TargetWorldY is not null;
        var payloadCount = (hasMovement ? 1 : 0) + (command.View is null ? 0 : 1) +
            (command.CursorMode is null ? 0 : 1) + (hasTarget ? 1 : 0);
        var valid = command.Kind switch
        {
            ExplorationCommandKind.ScrollCamera => payloadCount == 1 && hasMovement,
            ExplorationCommandKind.OpenView => payloadCount == 1 &&
                command.View is >= ExplorationView.ViewCharacter and <= ExplorationView.GameMenu,
            ExplorationCommandKind.SelectCursorMode => payloadCount == 1 &&
                command.CursorMode is not null && Enum.IsDefined(command.CursorMode.Value),
            ExplorationCommandKind.CenterCamera => payloadCount == 1 &&
                command.TargetWorldX is >= 0 && command.TargetWorldX < _worldWidth &&
                command.TargetWorldY is >= 0 && command.TargetWorldY < _worldHeight,
            ExplorationCommandKind.CycleCursorMode or ExplorationCommandKind.ShowLeaderOnly or
                ExplorationCommandKind.ShowExpandedParty or ExplorationCommandKind.Escape =>
                payloadCount == 0,
            _ => false
        };
        if (!valid)
            throw new ArgumentException("The exploration command payload does not match its kind.",
                nameof(command));
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

    private ExplorationSnapshot CenterOn(int worldX, int worldY) => _snapshot with
    {
        CameraX = Math.Clamp(worldX - _viewportWidth / 2, 0, _maximumCameraX),
        CameraY = Math.Clamp(worldY - _viewportHeight / 2, 0, _maximumCameraY),
        View = ExplorationView.World
    };
}
