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
    private static readonly StartWindowChoice[] Choices =
    [
        StartWindowChoice.StartGame,
        StartWindowChoice.CreateCharacters,
        StartWindowChoice.LoadSavedGame,
        StartWindowChoice.ExitToDos
    ];

    public static StartWindowChoice? HitTest(int logicalX, int logicalY)
    {
        for (var index = 0; index < OriginalContent.StartMenuButtons.Count; index++)
        {
            var button = OriginalContent.StartMenuButtons[index];
            if (logicalX >= button.X && logicalX < button.X + button.ControlWidth &&
                logicalY >= button.Y && logicalY < button.Y + button.ControlHeight)
                return Choices[index];
        }
        return null;
    }
}
