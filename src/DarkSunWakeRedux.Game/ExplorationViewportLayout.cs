namespace DarkSunWakeRedux.Game;

public readonly record struct ExplorationViewportLayout(
    int CameraX,
    int CameraY,
    int LogicalWidth,
    int LogicalHeight,
    int ViewportWidth,
    int ViewportHeight,
    bool Expanded)
{
    public static ExplorationViewportLayout Resolve(
        int viewportWidth,
        int viewportHeight,
        int worldWidth,
        int worldHeight,
        int cameraX,
        int cameraY,
        bool expand)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0 ||
            worldWidth < LogicalCanvasTransform.LogicalWidth ||
            worldHeight < LogicalCanvasTransform.LogicalHeight)
            throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        var maximumCameraX = worldWidth - LogicalCanvasTransform.LogicalWidth;
        var maximumCameraY = worldHeight - LogicalCanvasTransform.LogicalHeight;
        if (cameraX < 0 || cameraY < 0 ||
            cameraX > maximumCameraX || cameraY > maximumCameraY)
            throw new ArgumentOutOfRangeException(nameof(cameraX));
        if (!expand)
            return new(cameraX, cameraY,
                LogicalCanvasTransform.LogicalWidth,
                LogicalCanvasTransform.LogicalHeight,
                viewportWidth, viewportHeight, false);

        var scale = Math.Min(
            viewportWidth / (double)LogicalCanvasTransform.LogicalWidth,
            viewportHeight / (double)LogicalCanvasTransform.LogicalHeight);
        var logicalWidth = Math.Min(worldWidth, Math.Max(
            LogicalCanvasTransform.LogicalWidth,
            (int)Math.Ceiling(viewportWidth / scale)));
        var logicalHeight = Math.Min(worldHeight, Math.Max(
            LogicalCanvasTransform.LogicalHeight,
            (int)Math.Ceiling(viewportHeight / scale)));
        var centerX = cameraX + LogicalCanvasTransform.LogicalWidth / 2;
        var centerY = cameraY + LogicalCanvasTransform.LogicalHeight / 2;
        var expandedCameraX = Math.Clamp(centerX - logicalWidth / 2, 0,
            worldWidth - logicalWidth);
        var expandedCameraY = Math.Clamp(centerY - logicalHeight / 2, 0,
            worldHeight - logicalHeight);
        return new(expandedCameraX, expandedCameraY, logicalWidth, logicalHeight,
            viewportWidth, viewportHeight, true);
    }

    public bool TryToLogical(int viewportX, int viewportY, out int logicalX, out int logicalY)
    {
        logicalX = logicalY = 0;
        if (!Expanded)
            return new LogicalCanvasTransform(ViewportWidth, ViewportHeight)
                .TryToLogical(viewportX, viewportY, out logicalX, out logicalY);
        if (viewportX < 0 || viewportY < 0 ||
            viewportX >= ViewportWidth || viewportY >= ViewportHeight)
            return false;
        logicalX = Math.Min(LogicalWidth - 1,
            (int)((long)viewportX * LogicalWidth / ViewportWidth));
        logicalY = Math.Min(LogicalHeight - 1,
            (int)((long)viewportY * LogicalHeight / ViewportHeight));
        return true;
    }
}
