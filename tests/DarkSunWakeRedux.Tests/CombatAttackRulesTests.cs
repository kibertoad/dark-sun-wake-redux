using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class CombatAttackRulesTests
{
    [Theory]
    [InlineData(2, 5, 3, true)]
    [InlineData(1, 5, 3, false)]
    [InlineData(7, 5, -2, true)]
    [InlineData(6, 5, -2, false)]
    [InlineData(1, 0, 0, true)]
    public void ResolvesTheDocumentedInclusiveThac0Threshold(
        int roll,
        int thac0,
        int armorClass,
        bool expectedHits)
    {
        var check = CombatAttackRules.Resolve(roll, thac0, armorClass);

        Assert.Equal(thac0 - armorClass, check.RequiredRoll);
        Assert.Equal(expectedHits, check.Hits);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void RejectsRollsOutsideTheDocumentedOneThroughTwentyRange(int roll) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CombatAttackRules.Resolve(roll, 5, 3));

    [Fact]
    public void DoesNotOverflowForExtremeCombatStatistics()
    {
        var check = CombatAttackRules.Resolve(20, int.MaxValue, int.MinValue);

        Assert.Equal(int.MaxValue, check.RequiredRoll);
        Assert.False(check.Hits);
    }
}
