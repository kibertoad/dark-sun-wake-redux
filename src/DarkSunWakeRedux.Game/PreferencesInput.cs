using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public enum PreferencesAction
{
    MusicOnOff,
    MusicVolumeIncrease,
    MusicVolumeDecrease,
    SoundEffectsOnOff,
    SoundEffectsVolumeIncrease,
    SoundEffectsVolumeDecrease,
    AnimationsOnOff,
    About,
    DifficultyIncrease,
    DifficultyDecrease,
    GameMenu,
    ReturnToGame,
    VoiceEffectsOnOff
}

public sealed record PreferencesControl(
    PreferencesAction Action,
    uint ResourceNumber,
    int X,
    int Y,
    int Width,
    int Height,
    string AssetPath);

public static class PreferencesInput
{
    public static IReadOnlyList<PreferencesControl> Resolve(PackedUiCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var window = UiWindowGraphResolver.Resolve(
            catalog, OriginalContent.PreferencesWindowResourceNumber);
        var layer = OriginalContent.GameMenuLayer;
        if (window.Width != layer.FrameWidth || window.Height != layer.FrameHeight)
            throw new InvalidDataException(
                $"Preferences WIND #{window.ResourceNumber} has unexpected geometry.");
        var buttons = window.Controls.Where(control =>
            control.Kind == ResolvedUiControlKind.Button).ToArray();
        if (buttons.Length != OriginalContent.PreferencesButtons.Count)
            throw new InvalidDataException("The preferences graph has an unexpected button count.");

        var resolved = new List<PreferencesControl>(OriginalContent.PreferencesButtons.Count);
        foreach (var asset in OriginalContent.PreferencesButtons)
        {
            var matches = buttons.Where(item =>
                item.ResourceNumber == asset.ButtonResourceNumber).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException(
                    $"The preferences graph must contain exactly one BUTN #{asset.ButtonResourceNumber}.");
            var control = matches[0];
            if (control.X != asset.X || control.Y != asset.Y ||
                control.Width != asset.FrameWidth || control.Height != asset.FrameHeight ||
                control.ImageResourceNumber != asset.ImageResourceNumber)
                throw new InvalidDataException(
                    $"Preferences BUTN #{asset.ButtonResourceNumber} does not match its mapped geometry or image.");
            resolved.Add(new(Action(asset.Name), control.ResourceNumber,
                layer.X + control.X, layer.Y + control.Y,
                control.Width, control.Height, asset.Path));
        }
        return resolved;
    }

    public static PreferencesControl? HitTest(
        IReadOnlyList<PreferencesControl> controls, int logicalX, int logicalY) =>
        controls.LastOrDefault(control =>
            logicalX >= control.X && logicalX < control.X + control.Width &&
            logicalY >= control.Y && logicalY < control.Y + control.Height);

    public static ExplorationCommand? CommandFor(PreferencesControl control) => control.Action switch
    {
        PreferencesAction.GameMenu => ExplorationCommand.Open(ExplorationView.GameMenu),
        PreferencesAction.ReturnToGame => new(ExplorationCommandKind.Escape),
        _ => null
    };

    private static PreferencesAction Action(string name) => name switch
    {
        "music-on-off" => PreferencesAction.MusicOnOff,
        "music-volume-increase" => PreferencesAction.MusicVolumeIncrease,
        "music-volume-decrease" => PreferencesAction.MusicVolumeDecrease,
        "sound-effects-on-off" => PreferencesAction.SoundEffectsOnOff,
        "sound-effects-volume-increase" => PreferencesAction.SoundEffectsVolumeIncrease,
        "sound-effects-volume-decrease" => PreferencesAction.SoundEffectsVolumeDecrease,
        "animations-on-off" => PreferencesAction.AnimationsOnOff,
        "about" => PreferencesAction.About,
        "difficulty-increase" => PreferencesAction.DifficultyIncrease,
        "difficulty-decrease" => PreferencesAction.DifficultyDecrease,
        "game-menu" => PreferencesAction.GameMenu,
        "return-to-game" => PreferencesAction.ReturnToGame,
        "voice-effects-on-off" => PreferencesAction.VoiceEffectsOnOff,
        _ => throw new InvalidDataException($"Unknown mapped preferences action '{name}'.")
    };
}
