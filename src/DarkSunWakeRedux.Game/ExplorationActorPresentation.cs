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
        int cellPixelSize)
    {
        if (cellPixelSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellPixelSize),
                "Actor cell pixel size must be positive.");
        return new(checked(anchor.X * cellPixelSize + AnchorPixelOffsetX - cameraX),
            checked(anchor.Y * cellPixelSize + AnchorPixelOffsetY - cameraY),
            Width, Height);
    }

    public LogicalSpriteBounds AtMovement(
        ExplorationActorVisualSnapshot movement,
        int cameraX,
        int cameraY,
        int cellPixelSize)
    {
        if (movement.StepTicks <= 0 || movement.ProgressTicks < 0 ||
            movement.ProgressTicks > movement.StepTicks)
            throw new ArgumentOutOfRangeException(nameof(movement),
                "Actor interpolation progress must fit its positive step interval.");
        var anchorBounds = AtAnchor(
            movement.Anchor, cameraX, cameraY, cellPixelSize);
        var deltaX = checked(((long)movement.TargetAnchor.X - movement.Anchor.X) *
            cellPixelSize * movement.ProgressTicks / movement.StepTicks);
        var deltaY = checked(((long)movement.TargetAnchor.Y - movement.Anchor.Y) *
            cellPixelSize * movement.ProgressTicks / movement.StepTicks);
        return anchorBounds with
        {
            X = checked(anchorBounds.X + (int)deltaX),
            Y = checked(anchorBounds.Y + (int)deltaY)
        };
    }

    public (int X, int Y) WorldCenterAtAnchor(GridPoint anchor, int cellPixelSize)
    {
        var bounds = AtAnchor(anchor, 0, 0, cellPixelSize);
        return (checked(bounds.X + Width / 2), checked(bounds.Y + Height / 2));
    }

    public (int X, int Y) WorldCenterAtMovement(
        ExplorationActorVisualSnapshot movement,
        int cellPixelSize)
    {
        var bounds = AtMovement(movement, 0, 0, cellPixelSize);
        return (checked(bounds.X + Width / 2), checked(bounds.Y + Height / 2));
    }
}
