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
    [InlineData(CharacterOrigin.Mul, CharacterSex.Female, "mul_sex_invalid")]
    [InlineData(CharacterOrigin.ThriKreen, CharacterSex.Male, "thri_kreen_sex_invalid")]
    public void ManualSexRestrictionsAreEnforced(
        CharacterOrigin origin, CharacterSex sex, string expectedCode)
    {
        var diagnostics = PartyCreationRules.Validate(Draft(origin: origin, sex: sex));

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
    public void AgreedOriginClassProhibitionIsEnforced()
    {
        Assert.Equal(ClassEligibility.Prohibited,
            PartyCreationRules.Eligibility(CharacterOrigin.Dwarf, CharacterClass.Preserver));

        var diagnostics = PartyCreationRules.Validate(Draft(
            origin: CharacterOrigin.Dwarf, classes: [CharacterClass.Preserver]));
        Assert.Contains(diagnostics, item => item.Code == "class_origin_prohibited");
    }

    [Theory]
    [InlineData(CharacterOrigin.HalfGiant, CharacterClass.Ranger)]
    [InlineData(CharacterOrigin.Mul, CharacterClass.Druid)]
    [InlineData(CharacterOrigin.ThriKreen, CharacterClass.Druid)]
    [InlineData(CharacterOrigin.HalfGiant, CharacterClass.Thief)]
    public void ManualOriginAndClassSectionsRemainExplicitConflicts(
        CharacterOrigin origin, CharacterClass characterClass)
    {
        Assert.Equal(ClassEligibility.EvidenceConflict,
            PartyCreationRules.Eligibility(origin, characterClass));

        var diagnostics = PartyCreationRules.Validate(Draft(
            origin: origin,
            sex: origin == CharacterOrigin.ThriKreen ? CharacterSex.Female : CharacterSex.Male,
            classes: [characterClass]));
        Assert.Contains(diagnostics, item => item.Code == "class_origin_unresolved");
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
    public void ManualOriginAbilityModifiersAreRecordedWithoutGenerationAssumptions()
    {
        var expected = new Dictionary<CharacterOrigin, OriginAbilityModifiers>
        {
            [CharacterOrigin.Human] = new(0, 0, 0, 0, 0, 0),
            [CharacterOrigin.Dwarf] = new(1, -1, 2, 0, 0, -2),
            [CharacterOrigin.Elf] = new(0, 2, -2, 1, -1, 0),
            [CharacterOrigin.HalfElf] = new(0, 1, -1, 0, 0, 0),
            [CharacterOrigin.HalfGiant] = new(4, 0, 2, -2, -2, -2),
            [CharacterOrigin.Halfling] = new(-2, 2, -1, 0, 2, -1),
            [CharacterOrigin.Mul] = new(2, 0, 1, -1, 0, -2),
            [CharacterOrigin.ThriKreen] = new(0, 2, 0, -1, 1, -2)
        };

        Assert.Equal(Enum.GetValues<CharacterOrigin>(), expected.Keys.Order());
        foreach (var (origin, modifiers) in expected)
            Assert.Equal(modifiers, PartyCreationRules.AbilityModifiers(origin));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PartyCreationRules.AbilityModifiers((CharacterOrigin)999));
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
            .TryStartNext(CharacterOrigin.Human, CharacterClass.Preserver).Progression;
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
        Assert.Contains(PartyCreationRules.Validate(valid with { Origin = CharacterOrigin.Elf }),
            item => item.Code == "dual_class_human_only");
    }

    private static CharacterDraft Draft(
        string name = "Rikus",
        CharacterOrigin origin = CharacterOrigin.Human,
        CharacterSex sex = CharacterSex.Male,
        IReadOnlyList<CharacterClass>? classes = null,
        AbilityScores? abilities = null) =>
        new(name, origin, sex, CharacterAlignment.NeutralGood, abilities ?? new(15, 15, 15, 15, 15, 15),
            classes ?? [CharacterClass.Fighter]);
}
