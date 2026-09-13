using DarkSunWakeRedux.Core;
using Microsoft.Xna.Framework.Input;

namespace DarkSunWakeRedux.Game;

public sealed record ExplorationHotkeyBinding(Keys Key, ExplorationCommand Command);

public static class ExplorationHotkeys
{
    public static IReadOnlyList<ExplorationHotkeyBinding> Bindings { get; } =
    [
        new(Keys.Escape, new(ExplorationCommandKind.Escape)),
        new(Keys.Tab, ExplorationCommand.Open(ExplorationView.GameMenu)),
        new(Keys.V, ExplorationCommand.Open(ExplorationView.ViewCharacter)),
        new(Keys.I, ExplorationCommand.Open(ExplorationView.ViewInventory)),
        new(Keys.C, ExplorationCommand.Open(ExplorationView.CastSpellsOrUsePsionics)),
        new(Keys.U, ExplorationCommand.Open(ExplorationView.CastSpellsOrUsePsionics)),
        new(Keys.E, ExplorationCommand.Open(ExplorationView.CurrentSpellEffects)),
        new(Keys.O, ExplorationCommand.Open(ExplorationView.OverheadMap)),
        new(Keys.D5, new(ExplorationCommandKind.ShowExpandedParty)),
        new(Keys.D6, new(ExplorationCommandKind.ShowLeaderOnly))
    ];

    public static ExplorationCommand? Resolve(KeyboardState current, KeyboardState previous)
    {
        foreach (var binding in Bindings)
            if (current.IsKeyDown(binding.Key) && previous.IsKeyUp(binding.Key))
                return binding.Command;
        return null;
    }
}
