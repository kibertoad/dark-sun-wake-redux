using DarkSunWakeRedux.Core;

namespace DarkSunWakeRedux.Game;

public readonly record struct LogicalSpriteBounds(int X, int Y, int Width, int Height)
{
    public bool Intersects(int viewportWidth, int viewportHeight) =>
        Width > 0 && Height > 0 && X < viewportWidth && Y < viewportHeight &&
        X + Width > 0 && Y + Height > 0;
}

public sealed record ExplorationActorPresentation(
    int Width,
    int Height,
    int AnchorPixelOffsetX,
    int AnchorPixelOffsetY)
{
    public LogicalSpriteBounds AtAnchor(
        GridPoint anchor,
        int cameraX,
        int cameraY,
        int cellPixelSize) =>
        new(checked(anchor.X * cellPixelSize + AnchorPixelOffsetX - cameraX),
            checked(anchor.Y * cellPixelSize + AnchorPixelOffsetY - cameraY),
            Width, Height);
}
