using Microsoft.Xna.Framework.Input;

namespace DarkSunWakeRedux.Game;

/// <summary>
/// Requests an application screenshot once for each F12 key press.
/// </summary>
public static class ScreenshotInput
{
    public static bool ShouldCapture(KeyboardState current, KeyboardState previous) =>
        current.IsKeyDown(Keys.F12) && previous.IsKeyUp(Keys.F12);
}
