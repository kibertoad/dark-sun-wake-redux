using DarkSunWakeRedux.Core;

namespace DarkSunWakeRedux.Game;

public static class ExplorationInput
{
    public static ExplorationCommand? ScrollAtEdge(int logicalX, int logicalY)
    {
        if (logicalX < 0 || logicalX >= LogicalCanvasTransform.LogicalWidth ||
            logicalY < 0 || logicalY >= LogicalCanvasTransform.LogicalHeight)
            return null;
        var deltaX = logicalX switch
        {
            0 => -1,
            LogicalCanvasTransform.LogicalWidth - 1 => 1,
            _ => 0
        };
        var deltaY = logicalY switch
        {
            0 => -1,
            LogicalCanvasTransform.LogicalHeight - 1 => 1,
            _ => 0
        };
        return deltaX == 0 && deltaY == 0 ? null : ExplorationCommand.Scroll(deltaX, deltaY);
    }
}
