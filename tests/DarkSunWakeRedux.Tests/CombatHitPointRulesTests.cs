using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class CombatHitPointRulesTests
{
    [Theory]
    [InlineData(1, CombatVitalStatus.Conscious)]
    [InlineData(0, CombatVitalStatus.Unconscious)]
    [InlineData(-9, CombatVitalStatus.Unconscious)]
    [InlineData(-10, CombatVitalStatus.Dead)]
    [InlineData(-100, CombatVitalStatus.Dead)]
    public void ClassifiesTheDocumentedHitPointThresholds(
        int hitPoints,
        CombatVitalStatus expected) =>
        Assert.Equal(expected, CombatHitPointRules.StatusAt(hitPoints));

    [Fact]
    public void AppliesDamageBySubtraction()
    {
        var remaining = CombatHitPointRules.ApplyDamage(4, 4);

        Assert.Equal(0, remaining);
        Assert.Equal(CombatVitalStatus.Unconscious,
            CombatHitPointRules.StatusAt(remaining));
    }

    [Fact]
    public void SaturatesExtremeDamageInsteadOfWrapping() =>
        Assert.Equal(int.MinValue,
            CombatHitPointRules.ApplyDamage(int.MinValue + 1, int.MaxValue));

    [Fact]
    public void RejectsNegativeDamage() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CombatHitPointRules.ApplyDamage(4, -1));
}
