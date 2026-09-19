using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class CombatAttackEligibilityRulesTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void MeleeRequiresBothAdjacencyAndAReadiedWeapon(
        bool targetIsAdjacent,
        bool hasReadiedWeapon,
        bool expected) =>
        Assert.Equal(expected, CombatAttackEligibilityRules.CanMakeMeleeAttack(
            targetIsAdjacent, hasReadiedWeapon));

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void RangedRequiresBothRangeAndReadyRangedEquipment(
        bool targetIsInRange,
        bool hasReadiedMissileWeaponOrAmmunition,
        bool expected) =>
        Assert.Equal(expected, CombatAttackEligibilityRules.CanMakeRangedAttack(
            targetIsInRange, hasReadiedMissileWeaponOrAmmunition));
}
