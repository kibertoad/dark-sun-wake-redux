using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public enum GameMenuAction
{
    ViewCharacter,
    ViewInventory,
    CastSpellsOrUsePsionics,
    CurrentSpellEffects,
    ExitToDos,
    LoadSave,
    Preferences,
    OverheadMap,
    CenterOnLeader,
    CollapseParty,
    Walk,
    Look,
    Attack,
    ReturnToGame
}

public sealed record GameMenuControl(
    GameMenuAction Action,
    uint ResourceNumber,
    int X,
    int Y,
    int Width,
    int Height,
    string AssetPath);

public static class GameMenuInput
{
    public static IReadOnlyList<GameMenuControl> Resolve(PackedUiCatalog catalog)
    {
        var window = UiWindowGraphResolver.Resolve(
            catalog, OriginalContent.GameMenuWindowResourceNumber);
        var layer = OriginalContent.GameMenuLayer;
        if (window.Width != layer.FrameWidth || window.Height != layer.FrameHeight)
            throw new InvalidDataException(
                $"Game-menu WIND #{window.ResourceNumber} has unexpected geometry.");
        var buttons = window.Controls.Where(control =>
            control.Kind == ResolvedUiControlKind.Button).ToArray();
        if (buttons.Length != OriginalContent.GameMenuButtons.Count)
            throw new InvalidDataException("The game-menu graph has an unexpected button count.");

        var resolved = new List<GameMenuControl>(OriginalContent.GameMenuButtons.Count);
        foreach (var asset in OriginalContent.GameMenuButtons)
        {
            var matches = buttons.Where(item =>
                item.ResourceNumber == asset.ButtonResourceNumber).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException(
                    $"The game-menu graph must contain exactly one BUTN #{asset.ButtonResourceNumber}.");
            var control = matches[0];
            if (control.X != asset.X || control.Y != asset.Y ||
                control.Width != asset.FrameWidth || control.Height != asset.FrameHeight ||
                control.ImageResourceNumber != asset.ImageResourceNumber)
                throw new InvalidDataException(
                    $"Game-menu BUTN #{asset.ButtonResourceNumber} does not match its mapped geometry or image.");
            resolved.Add(new(Action(asset.Name), control.ResourceNumber,
                layer.X + control.X, layer.Y + control.Y,
                control.Width, control.Height, asset.Path));
        }
        return resolved;
    }

    public static GameMenuControl? HitTest(
        IReadOnlyList<GameMenuControl> controls, int logicalX, int logicalY) =>
        controls.LastOrDefault(control =>
            logicalX >= control.X && logicalX < control.X + control.Width &&
            logicalY >= control.Y && logicalY < control.Y + control.Height);

    public static ExplorationCommand? CommandFor(GameMenuControl control) => control.Action switch
    {
        GameMenuAction.ViewCharacter => ExplorationCommand.Open(ExplorationView.ViewCharacter),
        GameMenuAction.ViewInventory => ExplorationCommand.Open(ExplorationView.ViewInventory),
        GameMenuAction.CastSpellsOrUsePsionics =>
            ExplorationCommand.Open(ExplorationView.CastSpellsOrUsePsionics),
        GameMenuAction.CurrentSpellEffects =>
            ExplorationCommand.Open(ExplorationView.CurrentSpellEffects),
        // PLACEHOLDER: SCR-UI-006 - the original offers a choice to save, quit or cancel first.
        GameMenuAction.ExitToDos => ExplorationCommand.RequestExit(),
        GameMenuAction.Preferences => ExplorationCommand.Open(ExplorationView.Preferences),
        GameMenuAction.OverheadMap => ExplorationCommand.Open(ExplorationView.OverheadMap),
        GameMenuAction.CollapseParty =>
            new(ExplorationCommandKind.ShowExpandedParty),
        GameMenuAction.Walk => ExplorationCommand.SelectMode(ExplorationCursorMode.Walk),
        GameMenuAction.Look => ExplorationCommand.SelectMode(ExplorationCursorMode.Look),
        GameMenuAction.Attack => ExplorationCommand.SelectMode(ExplorationCursorMode.Attack),
        GameMenuAction.ReturnToGame => new(ExplorationCommandKind.Escape),
        _ => null
    };

    public static ExplorationCommand? CommandFor(
        GameMenuControl control,
        int leaderWorldX,
        int leaderWorldY) =>
        control.Action == GameMenuAction.CenterOnLeader
            ? ExplorationCommand.CenterOn(leaderWorldX, leaderWorldY)
            : CommandFor(control);

    private static GameMenuAction Action(string name) => name switch
    {
        "view-character" => GameMenuAction.ViewCharacter,
        "view-inventory" => GameMenuAction.ViewInventory,
        "cast-spells-use-psionics" => GameMenuAction.CastSpellsOrUsePsionics,
        "current-spell-effects" => GameMenuAction.CurrentSpellEffects,
        "exit-to-dos" => GameMenuAction.ExitToDos,
        "load-save" => GameMenuAction.LoadSave,
        "preferences" => GameMenuAction.Preferences,
        "overhead-map" => GameMenuAction.OverheadMap,
        "center-on-leader" => GameMenuAction.CenterOnLeader,
        "collapse-party" => GameMenuAction.CollapseParty,
        "walk" => GameMenuAction.Walk,
        "look" => GameMenuAction.Look,
        "attack" => GameMenuAction.Attack,
        "return-to-game" => GameMenuAction.ReturnToGame,
        _ => throw new InvalidDataException($"Unknown mapped game-menu action '{name}'.")
    };
}
