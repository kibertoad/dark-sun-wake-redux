using DarkSunWakeRedux.Core;

namespace DarkSunWakeRedux.Game;

/// <summary>
/// Keeps an active fixed-canvas dialogue and its world backdrop in one visual
/// coordinate system. This is presentation-only and never affects Core rules.
/// </summary>
public static class ExplorationWorldPresentation
{
    public static bool UsesExpandedWorld(
        ExplorationView view,
        bool dialogueVisible) =>
        view == ExplorationView.World && !dialogueVisible;
}
