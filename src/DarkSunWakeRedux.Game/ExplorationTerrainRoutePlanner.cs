using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public static class ExplorationTerrainRoutePlanner
{
    public static GridPathResult? PlanAt(
        RegionTerrainGrid terrain,
        ExplorationSnapshot snapshot,
        GridPoint start,
        int logicalX,
        int logicalY)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.View != ExplorationView.World ||
            snapshot.CursorMode != ExplorationCursorMode.Walk ||
            logicalX < 0 || logicalY < 0 ||
            logicalX >= LogicalCanvasTransform.LogicalWidth ||
            logicalY >= LogicalCanvasTransform.LogicalHeight)
            return null;

        var destinationCell = terrain.CellAtWorldPixel(
            checked(snapshot.CameraX + logicalX), checked(snapshot.CameraY + logicalY));
        return GridPathfinder.FindPath(terrain.Width, terrain.Height, start,
            new(destinationCell.X, destinationCell.Y),
            point => terrain.IsTerrainOpen(point.X, point.Y));
    }
}
