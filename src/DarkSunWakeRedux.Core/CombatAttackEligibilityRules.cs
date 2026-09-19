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

    /// <summary>
    /// A two-weapon melee configuration needs one one-handed weapon in each hand.
    /// Any attack penalty or exception is resolved separately from this shape check.
    /// </summary>
    public static bool CanReadyTwoMeleeWeapons(
        bool primaryWeaponIsOneHanded,
        bool offHandWeaponIsOneHanded) =>
        primaryWeaponIsOneHanded && offHandWeaponIsOneHanded;
}
