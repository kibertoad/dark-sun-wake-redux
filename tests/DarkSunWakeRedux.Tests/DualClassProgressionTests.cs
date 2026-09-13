using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DualClassProgressionTests
{
    [Fact]
    public void EligibleHumanStartsNewClassAtLevelOne()
    {
        var original = DualClassProgression.Begin(CharacterClass.Fighter, level: 3);

        var transition = original.TryStartNext(CharacterRace.Human, CharacterClass.Preserver);

        Assert.True(transition.Accepted);
        Assert.Equal([
            new CharacterClassLevel(CharacterClass.Fighter, 3),
            new CharacterClassLevel(CharacterClass.Preserver, 1)
        ], transition.Progression.Careers);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterClassLevel>)transition.Progression.Careers)[0] =
                new(CharacterClass.Gladiator, 9));
        Assert.Single(original.Careers);
    }

    [Theory]
    [InlineData(CharacterRace.Elf, CharacterClass.Preserver, 3, "dual_class_human_only")]
    [InlineData(CharacterRace.Human, CharacterClass.Preserver, 2, "dual_class_level_too_low")]
    [InlineData(CharacterRace.Human, CharacterClass.Fighter, 3, "dual_class_duplicate")]
    public void IneligibleTransitionReturnsDiagnosticsWithoutMutation(
        CharacterRace race,
        CharacterClass nextClass,
        int level,
        string code)
    {
        var original = DualClassProgression.Begin(CharacterClass.Fighter, level);

        var transition = original.TryStartNext(race, nextClass);

        Assert.False(transition.Accepted);
        Assert.Contains(transition.Diagnostics, item => item.Code == code);
        Assert.Same(original, transition.Progression);
    }

    [Fact]
    public void FormerBenefitsReactivateOnlyAfterNewClassExceedsFormerLevel()
    {
        var changed = DualClassProgression.Begin(CharacterClass.Fighter, 3)
            .TryStartNext(CharacterRace.Human, CharacterClass.Preserver).Progression;

        Assert.False(changed.AreCareerBenefitsActive(0));
        Assert.True(changed.AreCareerBenefitsActive(1));
        var equal = changed.TryAdvanceCurrentTo(3).Progression;
        Assert.False(equal.AreCareerBenefitsActive(0));
        var exceeded = equal.TryAdvanceCurrentTo(4).Progression;
        Assert.True(exceeded.AreCareerBenefitsActive(0));
    }

    [Fact]
    public void HumanMayChangeClassTwiceButNotThreeTimes()
    {
        var second = DualClassProgression.Begin(CharacterClass.Fighter, 3)
            .TryStartNext(CharacterRace.Human, CharacterClass.Thief).Progression
            .TryAdvanceCurrentTo(3).Progression;
        var third = second.TryStartNext(CharacterRace.Human, CharacterClass.Psionicist);

        Assert.True(third.Accepted);
        var rejected = third.Progression.TryAdvanceCurrentTo(3).Progression
            .TryStartNext(CharacterRace.Human, CharacterClass.Cleric);
        Assert.False(rejected.Accepted);
        Assert.Equal("dual_class_limit_reached", Assert.Single(rejected.Diagnostics).Code);
    }

    [Fact]
    public void AdvancementCannotMoveBackwardOrRepeatALevel()
    {
        var progression = DualClassProgression.Begin(CharacterClass.Fighter, 3);

        Assert.Equal("class_level_not_advanced",
            Assert.Single(progression.TryAdvanceCurrentTo(3).Diagnostics).Code);
        Assert.Equal("class_level_not_advanced",
            Assert.Single(progression.TryAdvanceCurrentTo(2).Diagnostics).Code);
        Assert.Throws<ArgumentOutOfRangeException>(() => progression.AreCareerBenefitsActive(1));
    }
}
