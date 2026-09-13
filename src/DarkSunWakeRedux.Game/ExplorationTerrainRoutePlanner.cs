using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public static class ExplorationTerrainRoutePlanner
{
    public static ExplorationMoveCommand? CommandAt(
        RegionTerrainGrid terrain,
        ExplorationSnapshot snapshot,
        int logicalX,
        int logicalY)
    {
        var destination = DestinationAt(terrain, snapshot, logicalX, logicalY);
        return destination is null ? null : ExplorationMoveCommand.Plan(destination.Value);
    }

    public static GridPathResult? PlanAt(
        RegionTerrainGrid terrain,
        ExplorationSnapshot snapshot,
        GridPoint start,
        int logicalX,
        int logicalY)
    {
        var command = CommandAt(terrain, snapshot, logicalX, logicalY);
        if (command is null) return null;
        return GridPathfinder.FindPath(terrain.Width, terrain.Height, start,
            command.Destination!.Value,
            point => terrain.IsTerrainOpen(point.X, point.Y));
    }

    private static GridPoint? DestinationAt(
        RegionTerrainGrid terrain,
        ExplorationSnapshot snapshot,
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
        var cell = terrain.CellAtWorldPixel(
            checked(snapshot.CameraX + logicalX), checked(snapshot.CameraY + logicalY));
        return new(cell.X, cell.Y);
    }
}
