namespace DarkSunWakeRedux.Core;

/// <summary>
/// Evaluates only the documented preconditions for a requested attack style.
/// Callers establish the physical relationship and readied equipment state.
/// </summary>
public static class CombatAttackEligibilityRules
{
    public static bool CanMakeMeleeAttack(bool targetIsAdjacent, bool hasReadiedWeapon) =>
        targetIsAdjacent && hasReadiedWeapon;

    public static bool CanMakeRangedAttack(
        bool targetIsInRange,
        bool hasReadiedMissileWeaponOrAmmunition) =>
        targetIsInRange && hasReadiedMissileWeaponOrAmmunition;
}
