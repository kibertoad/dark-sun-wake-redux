using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PartyCreationTests
{
    [Fact]
    public void LegalCharacterCanJoinPartyAndOneCharacterCanStart()
    {
        var party = new Party();

        var diagnostics = party.Add(Draft());

        Assert.Empty(diagnostics);
        Assert.True(party.CanStart);
        Assert.Single(party.Members);
    }

    [Fact]
    public void PartyCannotExceedFourCharacters()
    {
        var party = new Party();
        for (var index = 0; index < Party.MaximumSize; index++)
            Assert.Empty(party.Add(Draft(name: $"Hero {index}")));

        var diagnostic = Assert.Single(party.Add(Draft(name: "Fifth")));

        Assert.Equal("party_full", diagnostic.Code);
        Assert.Equal(Party.MaximumSize, party.Members.Count);
    }

    [Theory]
    [InlineData(CharacterRace.Mul, CharacterSex.Female, "mul_sex_invalid")]
    [InlineData(CharacterRace.ThriKreen, CharacterSex.Male, "thri_kreen_sex_invalid")]
    public void ManualSexRestrictionsAreEnforced(
        CharacterRace race, CharacterSex sex, string expectedCode)
    {
        var diagnostics = PartyCreationRules.Validate(Draft(race: race, sex: sex));

        Assert.Contains(diagnostics, item => item.Code == expectedCode);
    }

    [Fact]
    public void HumanCannotBeginAsMulticlassed()
    {
        var diagnostics = PartyCreationRules.Validate(Draft(classes:
            [CharacterClass.Fighter, CharacterClass.Thief]));

        Assert.Contains(diagnostics, item => item.Code == "human_multiclass_invalid");
    }

    [Fact]
    public void ClassAbilityMinimumIsEnforced()
    {
        var diagnostics = PartyCreationRules.Validate(Draft(
            classes: [CharacterClass.Psionicist],
            abilities: new(15, 15, 10, 11, 14, 15)));

        Assert.Equal(3, diagnostics.Count(item => item.Code == "class_ability_too_low"));
    }

    [Fact]
    public void AgreedRaceClassProhibitionIsEnforced()
    {
        Assert.Equal(ClassEligibility.Prohibited,
            PartyCreationRules.Eligibility(CharacterRace.Dwarf, CharacterClass.Preserver));

        var diagnostics = PartyCreationRules.Validate(Draft(
            race: CharacterRace.Dwarf, classes: [CharacterClass.Preserver]));
        Assert.Contains(diagnostics, item => item.Code == "class_race_prohibited");
    }

    [Theory]
    [InlineData(CharacterRace.HalfGiant, CharacterClass.Ranger)]
    [InlineData(CharacterRace.Mul, CharacterClass.Druid)]
    [InlineData(CharacterRace.ThriKreen, CharacterClass.Druid)]
    [InlineData(CharacterRace.HalfGiant, CharacterClass.Thief)]
    public void ManualRaceAndClassSectionsRemainExplicitConflicts(
        CharacterRace race, CharacterClass characterClass)
    {
        Assert.Equal(ClassEligibility.EvidenceConflict,
            PartyCreationRules.Eligibility(race, characterClass));

        var diagnostics = PartyCreationRules.Validate(Draft(
            race: race,
            sex: race == CharacterRace.ThriKreen ? CharacterSex.Female : CharacterSex.Male,
            classes: [characterClass]));
        Assert.Contains(diagnostics, item => item.Code == "class_race_unresolved");
    }

    [Theory]
    [InlineData(8)]
    [InlineData(25)]
    public void AbilityScoresOutsideManualRangeAreRejected(int strength)
    {
        var diagnostics = PartyCreationRules.Validate(Draft(abilities: new(strength, 15, 15, 15, 15, 15)));

        Assert.Contains(diagnostics, item => item.Code == "ability_out_of_range");
    }

    private static CharacterDraft Draft(
        string name = "Rikus",
        CharacterRace race = CharacterRace.Human,
        CharacterSex sex = CharacterSex.Male,
        IReadOnlyList<CharacterClass>? classes = null,
        AbilityScores? abilities = null) =>
        new(name, race, sex, CharacterAlignment.NeutralGood, abilities ?? new(15, 15, 15, 15, 15, 15),
            classes ?? [CharacterClass.Fighter]);
}
