using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public readonly record struct LogicalCanvasTransform(int ViewportWidth, int ViewportHeight)
{
    public const int LogicalWidth = 320;
    public const int LogicalHeight = 200;

    public float Scale => Math.Min(ViewportWidth / (float)LogicalWidth, ViewportHeight / (float)LogicalHeight);
    public int Width => Math.Max(1, (int)MathF.Floor(LogicalWidth * Scale));
    public int Height => Math.Max(1, (int)MathF.Floor(LogicalHeight * Scale));
    public int X => (ViewportWidth - Width) / 2;
    public int Y => (ViewportHeight - Height) / 2;

    public bool TryToLogical(int viewportX, int viewportY, out int logicalX, out int logicalY)
    {
        logicalX = logicalY = 0;
        if (ViewportWidth <= 0 || ViewportHeight <= 0 || viewportX < X || viewportY < Y ||
            viewportX >= X + Width || viewportY >= Y + Height)
            return false;
        logicalX = Math.Min(LogicalWidth - 1, (int)MathF.Floor((viewportX - X) / Scale));
        logicalY = Math.Min(LogicalHeight - 1, (int)MathF.Floor((viewportY - Y) / Scale));
        return true;
    }
}

public static class StartMenuInput
{
    public const uint WindowResourceNumber = 19500;

    private static readonly IReadOnlyDictionary<uint, StartWindowChoice> Choices =
        new Dictionary<uint, StartWindowChoice>
        {
            [19300] = StartWindowChoice.StartGame,
            [19301] = StartWindowChoice.CreateCharacters,
            [19302] = StartWindowChoice.LoadSavedGame,
            [19303] = StartWindowChoice.ExitToDos
        };

    public static IReadOnlyList<StartMenuControl> Resolve(PackedUiCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var window = UiWindowGraphResolver.Resolve(catalog, WindowResourceNumber);
        if (window.Width != LogicalCanvasTransform.LogicalWidth ||
            window.Height != LogicalCanvasTransform.LogicalHeight)
            throw new InvalidDataException(
                $"WIND #{WindowResourceNumber} has unexpected dimensions {window.Width}x{window.Height}.");

        var assets = OriginalContent.StartMenuButtons.ToDictionary(item => item.ButtonResourceNumber);
        var result = new List<StartMenuControl>(window.Controls.Count);
        foreach (var child in window.Controls)
        {
            if (child.Kind != ResolvedUiControlKind.Button ||
                !Choices.TryGetValue(child.ResourceNumber, out var choice) ||
                !assets.TryGetValue(child.ResourceNumber, out var asset))
                throw new InvalidDataException(
                    $"WIND #{WindowResourceNumber} contains unexpected child {child.Kind} #{child.ResourceNumber}.");
            if (child.ImageResourceNumber != asset.ImageResourceNumber)
                throw new InvalidDataException(
                    $"BUTN #{child.ResourceNumber} references unexpected ICON #{child.ImageResourceNumber}.");
            result.Add(new(choice, asset.Path, child.ResourceNumber, child.ImageResourceNumber.Value,
                child.X, child.Y, child.Width, child.Height));
        }
        if (result.Count != Choices.Count ||
            result.Select(item => item.ButtonResourceNumber).Distinct().Count() != Choices.Count)
            throw new InvalidDataException(
                $"WIND #{WindowResourceNumber} must contain each supported start control exactly once.");
        return result;
    }

    public static StartWindowChoice? HitTest(
        IReadOnlyList<StartMenuControl> controls,
        int logicalX,
        int logicalY)
    {
        ArgumentNullException.ThrowIfNull(controls);
        foreach (var control in controls)
            if (logicalX >= control.X && logicalX < control.X + control.Width &&
                logicalY >= control.Y && logicalY < control.Y + control.Height)
                return control.Choice;
        return null;
    }
}

public sealed record StartMenuControl(
    StartWindowChoice Choice,
    string AssetPath,
    uint ButtonResourceNumber,
    uint ImageResourceNumber,
    int X,
    int Y,
    int Width,
    int Height);
