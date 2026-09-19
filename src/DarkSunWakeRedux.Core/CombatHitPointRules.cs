namespace DarkSunWakeRedux.Core;

public enum CombatVitalStatus
{
    Conscious,
    Unconscious,
    Dead
}

/// <summary>
/// Encodes the manual's incapacity thresholds without assigning unevidenced
/// healing, stabilization, effect, or turn consequences.
/// </summary>
public static class CombatHitPointRules
{
    public const int UnconsciousAtOrBelow = 0;
    public const int DeadAtOrBelow = -10;

    public static CombatVitalStatus StatusAt(int hitPoints) => hitPoints switch
    {
        <= DeadAtOrBelow => CombatVitalStatus.Dead,
        <= UnconsciousAtOrBelow => CombatVitalStatus.Unconscious,
        _ => CombatVitalStatus.Conscious
    };

    public static int ApplyDamage(int hitPoints, int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), damage,
                "Damage cannot be negative.");
        var result = (long)hitPoints - damage;
        return result < int.MinValue ? int.MinValue : (int)result;
    }
}
