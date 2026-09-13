namespace DarkSunWakeRedux.Core;

public sealed record CharacterClassLevel(CharacterClass CharacterClass, int Level);

public sealed record DualClassTransition(
    DualClassProgression Progression,
    IReadOnlyList<PartyDiagnostic> Diagnostics)
{
    public bool Accepted => Diagnostics.Count == 0;
}

public sealed record DualClassProgression
{
    public const int MinimumCurrentLevel = 3;
    public const int MaximumCareers = 3;

    private readonly CharacterClassLevel[] _careers;

    private DualClassProgression(IEnumerable<CharacterClassLevel> careers) =>
        _careers = careers.Select(item => item with { }).ToArray();

    public IReadOnlyList<CharacterClassLevel> Careers => Array.AsReadOnly(_careers);
    public CharacterClassLevel Current => _careers[^1];

    public static DualClassProgression Begin(CharacterClass characterClass, int level = 1)
    {
        if (!Enum.IsDefined(characterClass))
            throw new ArgumentOutOfRangeException(nameof(characterClass));
        if (level < 1)
            throw new ArgumentOutOfRangeException(nameof(level), "Class level must be positive.");
        return new([new(characterClass, level)]);
    }

    public DualClassTransition TryStartNext(CharacterRace race, CharacterClass nextClass)
    {
        var diagnostics = ValidateCanStartNext(race).ToList();
        if (!Enum.IsDefined(nextClass))
            diagnostics.Add(new("dual_class_invalid", "The selected new class is invalid."));
        else if (_careers.Any(item => item.CharacterClass == nextClass))
            diagnostics.Add(new("dual_class_duplicate", "A character cannot return to a previous class."));

        return diagnostics.Count == 0
            ? new(new([.. _careers, new CharacterClassLevel(nextClass, 1)]), [])
            : new(this, diagnostics.ToArray());
    }

    public IReadOnlyList<PartyDiagnostic> ValidateCanStartNext(CharacterRace race)
    {
        var diagnostics = new List<PartyDiagnostic>();
        if (!Enum.IsDefined(race) || race != CharacterRace.Human)
            diagnostics.Add(new("dual_class_human_only", "Only humans may become dual-classed."));
        if (Current.Level < MinimumCurrentLevel)
            diagnostics.Add(new("dual_class_level_too_low",
                $"The current class must reach level {MinimumCurrentLevel} before changing class."));
        if (_careers.Length >= MaximumCareers)
            diagnostics.Add(new("dual_class_limit_reached",
                "A human may change class at most twice, for three total classes."));

        return diagnostics;
    }

    public DualClassTransition TryAdvanceCurrentTo(int level)
    {
        if (level <= Current.Level)
            return new(this,
                [new("class_level_not_advanced", "The new class level must exceed its current level.")]);
        var careers = _careers.ToArray();
        careers[^1] = careers[^1] with { Level = level };
        return new(new(careers), []);
    }

    public bool AreCareerBenefitsActive(int careerIndex)
    {
        if ((uint)careerIndex >= (uint)_careers.Length)
            throw new ArgumentOutOfRangeException(nameof(careerIndex));
        return careerIndex == _careers.Length - 1 || Current.Level > _careers[careerIndex].Level;
    }
}
