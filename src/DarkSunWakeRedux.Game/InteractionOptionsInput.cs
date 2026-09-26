using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record InteractionOptionControl(
    ExplorationInteractionAction Action,
    uint ResourceNumber,
    int X,
    int Y,
    int Width,
    int Height,
    bool Enabled,
    string AssetPath);

public static class InteractionOptionsInput
{
    // PLACEHOLDER: SCR-UI-011 - the original's Look panel icons match only with the origin at (67, 44).
    public const int ObservedOriginX = 68;
    public const int ObservedOriginY = 45;
    public const int WindowWidth = 92;
    public const int WindowHeight = 77;
    public const uint ApplicationFrameResourceNumber = 15200;

    private static readonly IReadOnlyDictionary<uint, (
        ExplorationInteractionAction Action, string AssetName, bool Enabled)> Mappings =
        new Dictionary<uint, (ExplorationInteractionAction, string, bool)>
        {
            [15306] = (ExplorationInteractionAction.Talk, "talk-disabled", false),
            [15308] = (ExplorationInteractionAction.PickUp, "pick-up-disabled", false),
            [15307] = (ExplorationInteractionAction.Use, "use-disabled", false),
            [15309] = (ExplorationInteractionAction.Dismiss, "dismiss", true)
        };

    public static IReadOnlyList<InteractionOptionControl> Resolve(PackedUiCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var window = UiWindowGraphResolver.Resolve(
            catalog, OriginalContent.HostileInteractionWindowResourceNumber);
        if (window.Width != WindowWidth || window.Height != WindowHeight ||
            window.ImageResourceNumber != 0)
            throw Error($"WIND #{window.ResourceNumber} has unexpected geometry or image");
        var frame = window.Controls.Where(control =>
            control.Kind == ResolvedUiControlKind.ApplicationFrame).ToArray();
        if (frame.Length != 1 || frame[0].ResourceNumber != ApplicationFrameResourceNumber ||
            frame[0].X != 0 || frame[0].Y != 0 || frame[0].Width != 145 ||
            frame[0].Height != 87 || frame[0].EventMask != 486)
            throw Error($"WIND #{window.ResourceNumber} does not contain the observed APFM contract");

        var buttons = window.Controls.Where(control =>
            control.Kind == ResolvedUiControlKind.Button).ToArray();
        if (buttons.Length != Mappings.Count)
            throw Error($"WIND #{window.ResourceNumber} has an unexpected button count");
        var assets = OriginalContent.InteractionButtonAssets.ToDictionary(
            asset => asset.Name, StringComparer.Ordinal);
        var controls = new List<InteractionOptionControl>(Mappings.Count);
        foreach (var button in buttons)
        {
            if (!Mappings.TryGetValue(button.ResourceNumber, out var mapping))
                throw Error($"WIND #{window.ResourceNumber} contains unexpected BUTN #{button.ResourceNumber}");
            var asset = assets[mapping.AssetName];
            if (button.ImageResourceNumber != asset.ImageResourceNumber)
                throw Error($"BUTN #{button.ResourceNumber} does not use mapped ICON #{asset.ImageResourceNumber}");
            controls.Add(new(mapping.Action, button.ResourceNumber,
                ObservedOriginX + button.X, ObservedOriginY + button.Y,
                button.Width, button.Height, mapping.Enabled, asset.Path));
        }
        if (controls.Select(control => control.Action).Distinct().Count() != Mappings.Count)
            throw Error("The hostile interaction graph does not resolve each action exactly once");
        return controls;
    }

    public static InteractionOptionControl? HitTest(
        IReadOnlyList<InteractionOptionControl> controls,
        int logicalX,
        int logicalY) => controls.LastOrDefault(control =>
            logicalX >= control.X && logicalX < control.X + control.Width &&
            logicalY >= control.Y && logicalY < control.Y + control.Height);

    private static InvalidDataException Error(string message) => new($"{message}.");
}
