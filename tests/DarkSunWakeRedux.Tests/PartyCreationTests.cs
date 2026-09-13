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

    [Fact]
    public void PsionicistMustReceiveAllThreeDisciplines()
    {
        var incomplete = Draft(classes: [CharacterClass.Psionicist]);

        Assert.Contains(PartyCreationRules.Validate(incomplete),
            item => item.Code == "psionicist_disciplines_invalid");
        Assert.DoesNotContain(PartyCreationRules.Validate(incomplete with
        {
            PsionicDisciplines = PsionicDisciplines.All
        }), item => item.Code == "psionicist_disciplines_invalid");
    }

    [Theory]
    [InlineData(PsionicDisciplines.None)]
    [InlineData(PsionicDisciplines.All)]
    public void NonPsionicistMustReceiveExactlyOneDiscipline(PsionicDisciplines disciplines)
    {
        var draft = Draft() with { PsionicDisciplines = disciplines };

        Assert.Contains(PartyCreationRules.Validate(draft),
            item => item.Code == "psionic_discipline_count_invalid");
    }

    [Fact]
    public void OnlyClericsChooseAnElementalSphere()
    {
        var cleric = Draft(classes: [CharacterClass.Cleric]);

        Assert.Contains(PartyCreationRules.Validate(cleric),
            item => item.Code == "clerical_sphere_missing");
        Assert.DoesNotContain(PartyCreationRules.Validate(cleric with
        {
            ClericalSphere = ClericalSphere.Air
        }), item => item.Code.StartsWith("clerical_sphere_", StringComparison.Ordinal));
        Assert.Contains(PartyCreationRules.Validate(Draft() with
        {
            ClericalSphere = ClericalSphere.Fire
        }), item => item.Code == "clerical_sphere_unavailable");
    }

    [Fact]
    public void ReplacementIsValidatedBeforeMutatingParty()
    {
        var party = new Party();
        Assert.Empty(party.Add(Draft()));

        Assert.Contains(party.ReplaceAt(0, Draft(name: "")),
            item => item.Code == "character_name_missing");
        Assert.Equal("Rikus", party.Members[0].Name);

        Assert.Empty(party.ReplaceAt(0, Draft(name: "Sadira")));
        Assert.Equal("Sadira", party.Members[0].Name);
    }

    [Fact]
    public void SequentialHumanCareersMustMatchProgressionState()
    {
        var progression = DualClassProgression.Begin(CharacterClass.Fighter, 3)
            .TryStartNext(CharacterRace.Human, CharacterClass.Preserver).Progression;
        var valid = Draft(classes: [CharacterClass.Fighter, CharacterClass.Preserver]) with
        {
            ClassProgression = progression
        };

        Assert.DoesNotContain(PartyCreationRules.Validate(valid),
            item => item.Code is "human_multiclass_invalid" or "dual_class_state_mismatch");
        Assert.Contains(PartyCreationRules.Validate(valid with
        {
            Classes = [CharacterClass.Fighter, CharacterClass.Thief]
        }), item => item.Code == "dual_class_state_mismatch");
        Assert.Contains(PartyCreationRules.Validate(valid with { Race = CharacterRace.Elf }),
            item => item.Code == "dual_class_human_only");
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
