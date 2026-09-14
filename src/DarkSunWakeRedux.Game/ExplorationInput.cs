using DarkSunWakeRedux.Core;

namespace DarkSunWakeRedux.Game;

public static class ExplorationInput
{
    public static ExplorationCommand? ScrollAtEdge(int logicalX, int logicalY)
        => ScrollAtEdge(logicalX, logicalY,
            LogicalCanvasTransform.LogicalWidth, LogicalCanvasTransform.LogicalHeight);

    public static ExplorationCommand? ScrollAtEdge(
        int logicalX,
        int logicalY,
        int logicalWidth,
        int logicalHeight)
    {
        if (logicalWidth <= 0 || logicalHeight <= 0 ||
            logicalX < 0 || logicalX >= logicalWidth ||
            logicalY < 0 || logicalY >= logicalHeight)
            return null;
        var deltaX = logicalX switch
        {
            0 => -1,
            _ when logicalX == logicalWidth - 1 => 1,
            _ => 0
        };
        var deltaY = logicalY switch
        {
            0 => -1,
            _ when logicalY == logicalHeight - 1 => 1,
            _ => 0
        };
        return deltaX == 0 && deltaY == 0 ? null : ExplorationCommand.Scroll(deltaX, deltaY);
    }
}
