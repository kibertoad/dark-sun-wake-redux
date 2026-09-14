using Microsoft.Xna.Framework.Input;

namespace DarkSunWakeRedux.Game;

public static class DialoguePreviewInput
{
    public static bool ShouldToggle(KeyboardState current, KeyboardState previous) =>
        current.IsKeyDown(Keys.F9) && previous.IsKeyUp(Keys.F9);
}
