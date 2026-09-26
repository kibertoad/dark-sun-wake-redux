using System.Numerics;

namespace DarkSunWakeRedux.Core;

public enum CharacterOrigin
{
    Human,
    Dwarf,
    Elf,
    HalfElf,
    HalfGiant,
    Halfling,
    Mul,
    ThriKreen
}

public enum CharacterSex { Male, Female }

public enum CharacterClass
{
    Fighter,
    Gladiator,
    Ranger,
    Preserver,
    Cleric,
    Druid,
    Thief,
    Psionicist
}

public enum CharacterAlignment
{
    LawfulGood,
    LawfulNeutral,
    NeutralGood,
    TrueNeutral,
    ChaoticGood,
    ChaoticNeutral
}

[Flags]
public enum PsionicDisciplines
{
    None = 0,
    Psychokinesis = 1,
    Psychometabolism = 2,
    Telepathy = 4,
    All = Psychokinesis | Psychometabolism | Telepathy
}

public enum ClericalSphere { Air, Earth, Fire, Water }

public enum ClassEligibility { Allowed, Prohibited, EvidenceConflict }

public sealed record AbilityScores(
    int Strength,
    int Dexterity,
    int Constitution,
    int Intelligence,
    int Wisdom,
    int Charisma)
{
    public IEnumerable<(string Name, int Value)> All()
    {
        yield return (nameof(Strength), Strength);
        yield return (nameof(Dexterity), Dexterity);
        yield return (nameof(Constitution), Constitution);
        yield return (nameof(Intelligence), Intelligence);
        yield return (nameof(Wisdom), Wisdom);
        yield return (nameof(Charisma), Charisma);
    }
}

public sealed record OriginAbilityModifiers(
    int Strength,
    int Dexterity,
    int Constitution,
    int Intelligence,
    int Wisdom,
    int Charisma);

public sealed record CharacterDraft(
    string Name,
    CharacterOrigin Origin,
    CharacterSex Sex,
    CharacterAlignment Alignment,
    AbilityScores Abilities,
    IReadOnlyList<CharacterClass> Classes)
{
    public PsionicDisciplines PsionicDisciplines { get; init; } = PsionicDisciplines.Psychokinesis;
    public ClericalSphere? ClericalSphere { get; init; }
    public DualClassProgression? ClassProgression { get; init; }
}

public sealed record PartyDiagnostic(string Code, string Message);

public sealed class Party
{
    public const int MaximumSize = 4;
    private readonly List<CharacterDraft> _members = [];

    public Party() { }

    internal Party(IEnumerable<CharacterDraft> members) => _members.AddRange(members);

    public IReadOnlyList<CharacterDraft> Members => _members;
    public bool CanStart => _members.Count is >= 1 and <= MaximumSize;

    public IReadOnlyList<PartyDiagnostic> Add(CharacterDraft character)
    {
        if (_members.Count == MaximumSize)
            return [new("party_full", "The party already contains four characters.")];
        var diagnostics = PartyCreationRules.Validate(character);
        if (diagnostics.Count == 0) _members.Add(character);
        return diagnostics;
    }

    public CharacterDraft RemoveAt(int index)
    {
        var member = _members[index];
        _members.RemoveAt(index);
        return member;
    }

    public IReadOnlyList<PartyDiagnostic> ReplaceAt(int index, CharacterDraft character)
    {
        if ((uint)index >= (uint)_members.Count)
            return [new("party_member_missing", "The selected party member does not exist.")];
        var diagnostics = PartyCreationRules.Validate(character);
        if (diagnostics.Count == 0) _members[index] = character;
        return diagnostics;
    }
}

public static class PartyCreationRules
{
    public const int MinimumAbility = 9;
    public const int MaximumAbility = 24;

    private static readonly IReadOnlyDictionary<CharacterClass, AbilityScores> Minimums =
        new Dictionary<CharacterClass, AbilityScores>
        {
            [CharacterClass.Fighter] = Scores(strength: 9),
            [CharacterClass.Gladiator] = Scores(strength: 13, dexterity: 12, constitution: 15),
            [CharacterClass.Ranger] = Scores(strength: 13, dexterity: 13, constitution: 14, wisdom: 14),
            [CharacterClass.Preserver] = Scores(intelligence: 9),
            [CharacterClass.Cleric] = Scores(wisdom: 9),
            [CharacterClass.Druid] = Scores(wisdom: 12, charisma: 15),
            [CharacterClass.Thief] = Scores(dexterity: 9),
            [CharacterClass.Psionicist] = Scores(constitution: 11, intelligence: 12, wisdom: 15)
        };

    private static readonly IReadOnlyDictionary<CharacterOrigin, OriginAbilityModifiers> OriginAbilityModifiersByOrigin =
        new Dictionary<CharacterOrigin, OriginAbilityModifiers>
        {
            [CharacterOrigin.Human] = Modifiers(),
            [CharacterOrigin.Dwarf] = Modifiers(strength: 1, dexterity: -1, constitution: 2, charisma: -2),
            [CharacterOrigin.Elf] = Modifiers(dexterity: 2, constitution: -2, intelligence: 1, wisdom: -1),
            [CharacterOrigin.HalfElf] = Modifiers(dexterity: 1, constitution: -1),
            [CharacterOrigin.HalfGiant] = Modifiers(strength: 4, constitution: 2,
                intelligence: -2, wisdom: -2, charisma: -2),
            [CharacterOrigin.Halfling] = Modifiers(strength: -2, dexterity: 2,
                constitution: -1, wisdom: 2, charisma: -1),
            [CharacterOrigin.Mul] = Modifiers(strength: 2, constitution: 1,
                intelligence: -1, charisma: -2),
            [CharacterOrigin.ThriKreen] = Modifiers(dexterity: 2,
                intelligence: -1, wisdom: 1, charisma: -2)
        };

    private static readonly IReadOnlyDictionary<CharacterOrigin, HashSet<CharacterClass>> OriginSectionClaims =
        new Dictionary<CharacterOrigin, HashSet<CharacterClass>>
        {
            [CharacterOrigin.Human] = Set(Enum.GetValues<CharacterClass>()),
            [CharacterOrigin.Dwarf] = Set(CharacterClass.Fighter, CharacterClass.Gladiator,
                CharacterClass.Cleric, CharacterClass.Thief, CharacterClass.Psionicist),
            [CharacterOrigin.Elf] = Set(CharacterClass.Fighter, CharacterClass.Gladiator,
                CharacterClass.Ranger, CharacterClass.Preserver, CharacterClass.Cleric,
                CharacterClass.Thief, CharacterClass.Psionicist),
            [CharacterOrigin.HalfElf] = Set(Enum.GetValues<CharacterClass>()),
            [CharacterOrigin.HalfGiant] = Set(CharacterClass.Fighter, CharacterClass.Gladiator,
                CharacterClass.Ranger, CharacterClass.Cleric, CharacterClass.Psionicist),
            [CharacterOrigin.Halfling] = Set(CharacterClass.Fighter, CharacterClass.Gladiator,
                CharacterClass.Ranger, CharacterClass.Cleric, CharacterClass.Druid,
                CharacterClass.Thief, CharacterClass.Psionicist),
            [CharacterOrigin.Mul] = Set(CharacterClass.Fighter, CharacterClass.Gladiator,
                CharacterClass.Cleric, CharacterClass.Thief, CharacterClass.Psionicist),
            [CharacterOrigin.ThriKreen] = Set(CharacterClass.Fighter, CharacterClass.Gladiator,
                CharacterClass.Ranger, CharacterClass.Cleric, CharacterClass.Psionicist)
        };

    private static readonly IReadOnlyDictionary<CharacterClass, HashSet<CharacterOrigin>> ClassSectionClaims =
        new Dictionary<CharacterClass, HashSet<CharacterOrigin>>
        {
            [CharacterClass.Fighter] = Set(Enum.GetValues<CharacterOrigin>()),
            [CharacterClass.Gladiator] = Set(Enum.GetValues<CharacterOrigin>()),
            [CharacterClass.Ranger] = Set(CharacterOrigin.Elf, CharacterOrigin.HalfElf,
                CharacterOrigin.Halfling, CharacterOrigin.Human, CharacterOrigin.ThriKreen),
            [CharacterClass.Preserver] = Set(CharacterOrigin.Elf, CharacterOrigin.HalfElf, CharacterOrigin.Human),
            [CharacterClass.Cleric] = Set(Enum.GetValues<CharacterOrigin>()),
            [CharacterClass.Druid] = Set(CharacterOrigin.HalfElf, CharacterOrigin.Halfling,
                CharacterOrigin.Human, CharacterOrigin.Mul, CharacterOrigin.ThriKreen),
            [CharacterClass.Thief] = Set(Enum.GetValues<CharacterOrigin>()),
            [CharacterClass.Psionicist] = Set(Enum.GetValues<CharacterOrigin>())
        };

    // PLACEHOLDER: RULE-PARTY-007 - which classes the game offers each origin is unknown; the
    // manual's two lists disagree, and README table 3 is a source the executable has not confirmed.
    public static ClassEligibility Eligibility(CharacterOrigin origin, CharacterClass characterClass)
    {
        var originAllows = OriginSectionClaims[origin].Contains(characterClass);
        var classAllows = ClassSectionClaims[characterClass].Contains(origin);
        return originAllows == classAllows
            ? originAllows ? ClassEligibility.Allowed : ClassEligibility.Prohibited
            : ClassEligibility.EvidenceConflict;
    }

    public static OriginAbilityModifiers AbilityModifiers(CharacterOrigin origin)
    {
        if (!Enum.IsDefined(origin)) throw new ArgumentOutOfRangeException(nameof(origin));
        return OriginAbilityModifiersByOrigin[origin];
    }

    public static IReadOnlyList<PartyDiagnostic> Validate(CharacterDraft character)
    {
        var diagnostics = new List<PartyDiagnostic>();
        if (string.IsNullOrWhiteSpace(character.Name))
            diagnostics.Add(new("character_name_missing", "A character name is required."));
        var validOrigin = Enum.IsDefined(character.Origin);
        if (!validOrigin || !Enum.IsDefined(character.Sex) || !Enum.IsDefined(character.Alignment))
            diagnostics.Add(new("character_option_invalid", "Origin, sex, or alignment is invalid."));
        if (character.Origin == CharacterOrigin.Mul && character.Sex != CharacterSex.Male)
            diagnostics.Add(new("mul_sex_invalid", "The manual permits only male mul player characters."));
        if (character.Origin == CharacterOrigin.ThriKreen && character.Sex != CharacterSex.Female)
            diagnostics.Add(new("thri_kreen_sex_invalid", "The manual permits only female thri-kreen player characters."));

        foreach (var (name, value) in character.Abilities.All())
            if (value is < MinimumAbility or > MaximumAbility)
                diagnostics.Add(new("ability_out_of_range", $"{name} must be between 9 and 24."));

        if (character.Classes is null || character.Classes.Count == 0)
            diagnostics.Add(new("class_missing", "At least one class is required."));
        else
        {
            if (character.Classes.Count > 3)
                diagnostics.Add(new("class_count_invalid", "At most three classes may be selected."));
            var hasSequentialCareers = character.ClassProgression is { Careers.Count: > 1 };
            if (character.Origin == CharacterOrigin.Human && character.Classes.Count != 1 &&
                !hasSequentialCareers)
                diagnostics.Add(new("human_multiclass_invalid", "Humans begin with one class and may dual-class later."));
            if (character.Classes.Distinct().Count() != character.Classes.Count)
                diagnostics.Add(new("class_duplicate", "A class cannot be selected more than once."));
            if (character.Classes.Contains(CharacterClass.Cleric) && character.Classes.Contains(CharacterClass.Druid))
                diagnostics.Add(new("cleric_druid_incompatible", "Cleric and druid cannot be selected together."));

            foreach (var selectedClass in character.Classes.Distinct())
            {
                if (!Enum.IsDefined(selectedClass))
                {
                    diagnostics.Add(new("class_invalid", "A selected class is invalid."));
                    continue;
                }
                if (validOrigin)
                {
                    var eligibility = Eligibility(character.Origin, selectedClass);
                    if (eligibility == ClassEligibility.Prohibited)
                        diagnostics.Add(new("class_origin_prohibited", $"{character.Origin} cannot be a {selectedClass}."));
                    if (eligibility == ClassEligibility.EvidenceConflict)
                        diagnostics.Add(new("class_origin_unresolved", $"Manual sections conflict about {character.Origin} {selectedClass} eligibility."));
                }
                ValidateMinimums(selectedClass, character.Abilities, diagnostics);
            }
        }
        ValidateClassProgression(character, diagnostics);
        ValidatePsionicsAndSphere(character, diagnostics);
        return diagnostics;
    }

    private static void ValidateClassProgression(
        CharacterDraft character, ICollection<PartyDiagnostic> diagnostics)
    {
        if (character.ClassProgression is null) return;
        if (character.Origin != CharacterOrigin.Human)
            diagnostics.Add(new("dual_class_human_only", "Only humans may carry dual-class progression."));
        if (character.Classes is null || !character.Classes.SequenceEqual(
                character.ClassProgression.Careers.Select(item => item.CharacterClass)))
            diagnostics.Add(new("dual_class_state_mismatch",
                "Selected classes must match the ordered dual-class careers."));
    }

    private static void ValidatePsionicsAndSphere(
        CharacterDraft character, ICollection<PartyDiagnostic> diagnostics)
    {
        var disciplines = character.PsionicDisciplines;
        var validFlags = (disciplines & ~PsionicDisciplines.All) == 0;
        var selectedCount = validFlags ? BitOperations.PopCount((uint)disciplines) : 0;
        if (character.Classes?.Contains(CharacterClass.Psionicist) == true)
        {
            if (!validFlags || disciplines != PsionicDisciplines.All)
                diagnostics.Add(new("psionicist_disciplines_invalid",
                    "Psionicists must specialize in all three psionic disciplines."));
        }
        else if (!validFlags || selectedCount != 1)
            diagnostics.Add(new("psionic_discipline_count_invalid",
                "A non-psionicist must select exactly one psionic discipline."));

        var isCleric = character.Classes?.Contains(CharacterClass.Cleric) == true;
        if (isCleric && character.ClericalSphere is null)
            diagnostics.Add(new("clerical_sphere_missing", "A cleric must select an elemental sphere."));
        else if (character.ClericalSphere is { } sphere && !Enum.IsDefined(sphere))
            diagnostics.Add(new("clerical_sphere_invalid", "The selected clerical sphere is invalid."));
        // PLACEHOLDER: RULE-PARTY-003 - the manual also gives druids and rangers a sphere; which
        // classes the game lets choose one is unknown.
        else if (!isCleric && character.ClericalSphere is not null)
            diagnostics.Add(new("clerical_sphere_unavailable", "Only clerics select a clerical sphere."));
    }

    private static void ValidateMinimums(
        CharacterClass characterClass, AbilityScores actual, ICollection<PartyDiagnostic> diagnostics)
    {
        var required = Minimums[characterClass];
        foreach (var ((name, value), (_, minimum)) in actual.All().Zip(required.All()))
            if (value < minimum)
                diagnostics.Add(new("class_ability_too_low", $"{characterClass} requires {name} {minimum} or higher."));
    }

    private static AbilityScores Scores(
        int strength = MinimumAbility,
        int dexterity = MinimumAbility,
        int constitution = MinimumAbility,
        int intelligence = MinimumAbility,
        int wisdom = MinimumAbility,
        int charisma = MinimumAbility) =>
        new(strength, dexterity, constitution, intelligence, wisdom, charisma);

    private static OriginAbilityModifiers Modifiers(
        int strength = 0,
        int dexterity = 0,
        int constitution = 0,
        int intelligence = 0,
        int wisdom = 0,
        int charisma = 0) =>
        new(strength, dexterity, constitution, intelligence, wisdom, charisma);

    private static HashSet<T> Set<T>(params T[] values) where T : struct, Enum => [.. values];
}
