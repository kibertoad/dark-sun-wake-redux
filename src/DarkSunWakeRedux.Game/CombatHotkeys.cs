using DarkSunWakeRedux.Core;
using Microsoft.Xna.Framework.Input;

namespace DarkSunWakeRedux.Game;

public sealed record CombatHotkeyBinding(Keys Key, CombatCommand Command);

/// <summary>
/// Maps the manual's combat-only keyboard requests without assigning their
/// unresolved turn, target, or automation semantics.
/// </summary>
public static class CombatHotkeys
{
    public static IReadOnlyList<CombatHotkeyBinding> Bindings { get; } =
    [
        new(Keys.G, new(CombatCommandKind.Guard)),
        new(Keys.N, new(CombatCommandKind.TargetNext)),
        new(Keys.P, new(CombatCommandKind.TargetPrevious)),
        new(Keys.Q, new(CombatCommandKind.EndTurn)),
        new(Keys.W, new(CombatCommandKind.Wait)),
        new(Keys.Space, new(CombatCommandKind.DisableComputerControl))
    ];

    public static CombatCommand? Resolve(KeyboardState current, KeyboardState previous)
    {
        foreach (var binding in Bindings)
            if (current.IsKeyDown(binding.Key) && previous.IsKeyUp(binding.Key))
                return binding.Command;
        return null;
    }
}
