using DarkSunWakeRedux.Core;

namespace DarkSunWakeRedux.Game;

public enum ExplorationCursorVisual
{
    Walk,
    CannotWalk,
    MeleeAttack,
    CannotMeleeAttack,
    RangedAttack,
    CannotRangedAttack,
    Look,
    CannotLook,
    CannotCast,
    Wait
}

public static class ExplorationCursorFeedback
{
    public static ExplorationCursorVisual Resolve(
        ExplorationSnapshot snapshot,
        bool walkReachable,
        bool meleeTarget,
        bool lookTarget)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.View != ExplorationView.World)
            return ExplorationCursorVisual.Walk;
        return snapshot.CursorMode switch
        {
            ExplorationCursorMode.Walk => walkReachable
                ? ExplorationCursorVisual.Walk
                : ExplorationCursorVisual.CannotWalk,
            // PLACEHOLDER: RULE-INPUT-002 - the original also shows a ranged-attack pair, which its
            // captures show over the first hostile character.
            ExplorationCursorMode.Attack => meleeTarget
                ? ExplorationCursorVisual.MeleeAttack
                : ExplorationCursorVisual.CannotMeleeAttack,
            ExplorationCursorMode.Look => lookTarget
                ? ExplorationCursorVisual.Look
                : ExplorationCursorVisual.CannotLook,
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot),
                $"Unsupported exploration cursor mode {snapshot.CursorMode}.")
        };
    }

    public static string AssetName(ExplorationCursorVisual visual) => visual switch
    {
        ExplorationCursorVisual.Walk => "walk",
        ExplorationCursorVisual.CannotWalk => "cannot-walk",
        ExplorationCursorVisual.MeleeAttack => "melee-attack",
        ExplorationCursorVisual.CannotMeleeAttack => "cannot-melee-attack",
        ExplorationCursorVisual.RangedAttack => "ranged-attack",
        ExplorationCursorVisual.CannotRangedAttack => "cannot-ranged-attack",
        ExplorationCursorVisual.Look => "look",
        ExplorationCursorVisual.CannotLook => "cannot-look",
        ExplorationCursorVisual.CannotCast => "cannot-cast",
        ExplorationCursorVisual.Wait => "wait",
        _ => throw new ArgumentOutOfRangeException(nameof(visual), visual, null)
    };
}
