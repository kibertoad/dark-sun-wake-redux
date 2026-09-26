using DarkSunWakeRedux.Core;

namespace DarkSunWakeRedux.Game;

/// <summary>
/// Keeps an active fixed-canvas dialogue and its world backdrop in one visual
/// coordinate system. This is presentation-only and never affects Core rules.
/// </summary>
public static class ExplorationWorldPresentation
{
    /// <summary>
    /// Whether the map view grows to fill the display (DEV-EXPLORE-001). The camera, the pointer
    /// mapping, edge scrolling, route clicks and the F9 preview all take their layout from this
    /// answer, so with <paramref name="wideMapView"/> off every one of them uses the original's
    /// 320x200 framing, centred and letterboxed.
    /// </summary>
    public static bool UsesExpandedWorld(
        ExplorationView view,
        bool dialogueVisible,
        bool wideMapView) =>
        wideMapView && view == ExplorationView.World && !dialogueVisible;
}
