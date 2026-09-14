using Microsoft.Xna.Framework.Input;

namespace DarkSunWakeRedux.Game;

public sealed record FullscreenTarget(bool IsFullScreen, int Width, int Height);

public static class FullscreenInput
{
    public const int WindowedWidth = 960;
    public const int WindowedHeight = 600;

    public static bool ShouldToggle(KeyboardState current, KeyboardState previous) =>
        ChordIsDown(current) && !ChordIsDown(previous);

    private static bool ChordIsDown(KeyboardState keyboard) =>
        keyboard.IsKeyDown(Keys.Enter) &&
        (keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt));

    public static FullscreenTarget ResolveTarget(
        bool isCurrentlyFullscreen,
        int displayWidth,
        int displayHeight)
    {
        if (displayWidth <= 0 || displayHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(displayWidth),
                "Display dimensions must be positive.");
        return isCurrentlyFullscreen
            ? new(false, WindowedWidth, WindowedHeight)
            : new(true, displayWidth, displayHeight);
    }
}
