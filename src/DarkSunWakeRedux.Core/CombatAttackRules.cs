namespace DarkSunWakeRedux.Core;

public sealed record CombatAttackCheck(
    int Roll,
    int AttackerThac0,
    int TargetArmorClass,
    int RequiredRoll,
    bool Hits);

/// <summary>
/// Resolves only the manual's documented THAC0-versus-armor-class threshold.
/// RNG, modifier application, damage, and critical-hit behavior are deliberately
/// separate concerns pending their own evidence.
/// </summary>
public static class CombatAttackRules
{
    public const int MinimumRoll = 1;
    public const int MaximumRoll = 20;

    public static CombatAttackCheck Resolve(
        int roll,
        int attackerThac0,
        int targetArmorClass)
    {
        if (roll is < MinimumRoll or > MaximumRoll)
            throw new ArgumentOutOfRangeException(nameof(roll), roll,
                "An attack roll must be from 1 through 20.");

        var requiredRoll = (long)attackerThac0 - targetArmorClass;
        var hits = roll >= requiredRoll;
        return new(roll, attackerThac0, targetArmorClass,
            ClampToInt(requiredRoll), hits);
    }

    private static int ClampToInt(long value) =>
        value switch
        {
            > int.MaxValue => int.MaxValue,
            < int.MinValue => int.MinValue,
            _ => (int)value
        };
}
