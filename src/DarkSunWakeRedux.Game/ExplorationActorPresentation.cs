namespace DarkSunWakeRedux.Game;

public readonly record struct LogicalSpriteBounds(int X, int Y, int Width, int Height)
{
    public bool Intersects(int viewportWidth, int viewportHeight) =>
        Width > 0 && Height > 0 && X < viewportWidth && Y < viewportHeight &&
        X + Width > 0 && Y + Height > 0;
}

public sealed record ExplorationActorPresentation(
    int WorldX,
    int WorldY,
    int Width,
    int Height)
{
    public LogicalSpriteBounds AtCamera(int cameraX, int cameraY) =>
        new(WorldX - cameraX, WorldY - cameraY, Width, Height);
}
